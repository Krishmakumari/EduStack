// EnrollmentService — Core business logic for student enrollment and progress tracking.
// • Handles enrollment creation, duplicate prevention, and lesson completion tracking.
// • Progress percentage calculated in memory after eager loading LessonProgresses.
// • Ownership validation on every write: studentId from JWT must match enrollment's StudentId.

using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.DTOs.Responses;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Exceptions;
using EnrollmentService.Infrastructure.Messaging;
using EnrollmentService.Domain.Events;

namespace EnrollmentService.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    // Two repositories — one per entity.
    // We need both because marking a lesson complete affects LessonProgress,
    // but we must first verify the enrollment exists and belongs to the student.
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ILessonProgressRepository _progressRepo;
    private readonly RabbitMqPublisher _publisher;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepo,
        ILessonProgressRepository progressRepo,
        RabbitMqPublisher publisher)
    {
        _enrollmentRepo = enrollmentRepo;
        _progressRepo = progressRepo;
        _publisher = publisher;
    }

    // ─── Enroll ───────────────────────────────────────────────────────────────
    // Registers a student in a course. studentId and studentName come from JWT claims.
    // CourseTitle is passed in the request (denormalized) to avoid calling Course Service.
    // DUPLICATE CHECK: Application-level guard (DB also has a unique index as a safety net).
    public async Task<EnrollmentResponse> EnrollAsync(
        Guid studentId, string studentName, string studentEmail, EnrollRequest request)
    {
        // Step 1: Check if already enrolled in this course.
        // Uses composite lookup by studentId + courseId — unique enrollment only.
        var existing = await _enrollmentRepo
            .GetByStudentAndCourseAsync(studentId, request.CourseId);

        if (existing is not null)
            throw new AlreadyEnrolledException();  // → 400 Bad Request via middleware

        // Step 2: Create enrollment via factory — sets Status = Active, EnrolledAt = UtcNow.
        var enrollment = Enrollment.Create(
            studentId,
            studentName,       // denormalized from JWT — no Auth Service call needed
            request.CourseId,
            request.CourseTitle, // denormalized from request — no Course Service call needed
            request.PricePaid);  // amount paid at time of enrollment (for history)

        await _enrollmentRepo.AddAsync(enrollment);
        await _enrollmentRepo.SaveChangesAsync();

        // Step 3: Publish "EnrollmentCompletedEvent" to RabbitMQ.
        // NotificationService will pick this up and send an email.
        await _publisher.PublishAsync("enrollment_queue", new EnrollmentCompletedEvent
        {
            UserId = studentId,
            Email = studentEmail,
            CourseTitle = request.CourseTitle
        });

        return MapToEnrollmentResponse(enrollment);
    }

    // ─── Get My Enrollments ───────────────────────────────────────────────────
    // Returns all courses the student is currently enrolled in.
    // studentId from JWT — no student can see another's enrollments.
    // Returns lightweight EnrollmentResponse (no lesson data) for faster listing.
    public async Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepo.GetByStudentIdAsync(studentId);
        return enrollments.Select(MapToEnrollmentResponse);
    }

    // ─── Get Enrollment By ID (Detail View) ───────────────────────────────────
    // Returns the full enrollment with all LessonProgress records.
    // OWNERSHIP: Validates the JWT student is the enrollment's owner.
    // Uses GetByIdWithProgressAsync — eager loads LessonProgresses via .Include().
    public async Task<EnrollmentDetailResponse> GetEnrollmentByIdAsync(
        Guid studentId, Guid enrollmentId)
    {
        // Must use the "WithProgress" version — response includes lesson progress list.
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        // Ownership check — Student A cannot view Student B's enrollment detail.
        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        return MapToEnrollmentDetailResponse(enrollment);
    }

    // ─── Get Enrollments By Course (Instructor View) ───────────────────────────
    // Returns all students enrolled in a specific course — for Instructor/Admin dashboards.
    // No ownership check here: Instructor needs to see ALL students in their course.
    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepo.GetByCourseIdAsync(courseId);
        return enrollments.Select(MapToEnrollmentResponse);
    }

    // ─── Mark Lesson Complete ─────────────────────────────────────────────────
    // Marks a specific lesson as completed for this student's enrollment.
    // UPSERT LOGIC: Create a new LessonProgress record if none exists, or update existing.
    // IDEMPOTENCY GUARD: Throws if the lesson is already marked complete.
    public async Task<ProgressResponse> MarkLessonCompleteAsync(
        Guid studentId, Guid enrollmentId, Guid lessonId)
    {
        // Step 1: Load enrollment with all current progress records.
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        // Step 2: Ownership check — only the enrolled student can mark lessons complete.
        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        // Step 3: Check if a LessonProgress record already exists for this lesson.
        var existing = await _progressRepo
            .GetByEnrollmentAndLessonAsync(enrollmentId, lessonId);

        // Step 4: Guard against double-completion.
        if (existing is not null && existing.IsCompleted)
            throw new DomainException("Lesson already marked as complete.");

        if (existing is null)
        {
            // First time marking — create new LessonProgress record.
            var progress = LessonProgress.Create(enrollmentId, lessonId);
            progress.MarkCompleted();       // sets IsCompleted = true, CompletedAt = UtcNow
            await _progressRepo.AddAsync(progress);
        }
        else
        {
            // Record exists but not complete — update it.
            existing.MarkCompleted();
        }

        await _progressRepo.SaveChangesAsync();

        // Step 5: Reload enrollment to get the freshest LessonProgress data.
        // We reload because the in-memory collection may not include the new record yet.
        var updated = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();
        return MapToProgressResponse(updated);
    }

    // ─── Get Progress ─────────────────────────────────────────────────────────
    // Returns progress statistics: total lessons, completed, and percentage.
    // Calculation happens in memory after loading the LessonProgresses collection.
    public async Task<ProgressResponse> GetProgressAsync(Guid studentId, Guid enrollmentId)
    {
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        // Ownership check — only the student can see their own progress.
        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        return MapToProgressResponse(enrollment);
    }

    public async Task<bool> IsUserEnrolledAsync(Guid studentId, Guid courseId)
    {
        var enrollment = await _enrollmentRepo.GetByStudentAndCourseAsync(studentId, courseId);
        return enrollment != null;
    }

    // ─── Mapping Helpers ──────────────────────────────────────────────────────
    // Manual mapping instead of AutoMapper — simpler, faster, easier to debug.
    // Enum Status is converted to string for JSON-friendly API responses.

    // Lightweight response — for listing (no lesson progress data).
    private static EnrollmentResponse MapToEnrollmentResponse(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        StudentName = e.StudentName,
        CourseId = e.CourseId,
        CourseTitle = e.CourseTitle,
        PricePaid = e.PricePaid,
        Status = e.Status.ToString(),   // enum → string for JSON
        EnrolledAt = e.EnrolledAt,
        CompletedAt = e.CompletedAt     // null if not completed yet
    };

    // Full response — for detail view (includes all lesson progress records).
    private static EnrollmentDetailResponse MapToEnrollmentDetailResponse(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        StudentName = e.StudentName,
        CourseId = e.CourseId,
        CourseTitle = e.CourseTitle,
        PricePaid = e.PricePaid,
        Status = e.Status.ToString(),
        EnrolledAt = e.EnrolledAt,
        CompletedAt = e.CompletedAt,
        LessonProgresses = e.LessonProgresses
            .Select(p => new LessonProgressResponse
            {
                LessonProgressId = p.LessonProgressId,
                LessonId = p.LessonId,
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt
            }).ToList()
    };

    // Progress stats response — calculates % complete in memory.
    private static ProgressResponse MapToProgressResponse(Enrollment e)
    {
        var total = e.LessonProgresses.Count;
        var completed = e.LessonProgresses.Count(p => p.IsCompleted);

        // Guard against division by zero — returns 0% if no lessons are tracked yet.
        var percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 2);

        return new ProgressResponse
        {
            EnrollmentId = e.EnrollmentId,
            CourseId = e.CourseId,
            TotalLessons = total,
            CompletedLessons = completed,
            ProgressPercentage = percentage,  // e.g. 66.67
            Status = e.Status.ToString(),
            LessonProgresses = e.LessonProgresses
                .Select(p => new LessonProgressResponse
                {
                    LessonProgressId = p.LessonProgressId,
                    LessonId = p.LessonId,
                    IsCompleted = p.IsCompleted,
                    CompletedAt = p.CompletedAt
                }).ToList()
        };
    }
}