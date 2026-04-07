using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.DTOs.Responses;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Exceptions;

namespace EnrollmentService.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ILessonProgressRepository _progressRepo;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepo,
        ILessonProgressRepository progressRepo)
    {
        _enrollmentRepo = enrollmentRepo;
        _progressRepo = progressRepo;
    }

    // ─── Enroll ───────────────────────────────────────────────────────────────
    public async Task<EnrollmentResponse> EnrollAsync(
        Guid studentId, string studentName, EnrollRequest request)
    {
        // Check if already enrolled
        var existing = await _enrollmentRepo
            .GetByStudentAndCourseAsync(studentId, request.CourseId);

        if (existing is not null)
            throw new AlreadyEnrolledException();

        var enrollment = Enrollment.Create(
            studentId,
            studentName,
            request.CourseId,
            request.CourseTitle,
            request.PricePaid);

        await _enrollmentRepo.AddAsync(enrollment);
        await _enrollmentRepo.SaveChangesAsync();

        return MapToEnrollmentResponse(enrollment);
    }

    // ─── Get My Enrollments ───────────────────────────────────────────────────
    public async Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepo.GetByStudentIdAsync(studentId);
        return enrollments.Select(MapToEnrollmentResponse);
    }

    // ─── Get Enrollment By ID ─────────────────────────────────────────────────
    public async Task<EnrollmentDetailResponse> GetEnrollmentByIdAsync(
        Guid studentId, Guid enrollmentId)
    {
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        return MapToEnrollmentDetailResponse(enrollment);
    }

    // ─── Get Enrollments By Course (Instructor) ───────────────────────────────
    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepo.GetByCourseIdAsync(courseId);
        return enrollments.Select(MapToEnrollmentResponse);
    }

    // ─── Mark Lesson Complete ─────────────────────────────────────────────────
    public async Task<ProgressResponse> MarkLessonCompleteAsync(
        Guid studentId, Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        // Check if already marked complete
        var existing = await _progressRepo
            .GetByEnrollmentAndLessonAsync(enrollmentId, lessonId);

        if (existing is not null && existing.IsCompleted)
            throw new DomainException("Lesson already marked as complete.");

        if (existing is null)
        {
            var progress = LessonProgress.Create(enrollmentId, lessonId);
            progress.MarkCompleted();
            await _progressRepo.AddAsync(progress);
        }
        else
        {
            existing.MarkCompleted();
        }

        await _progressRepo.SaveChangesAsync();

        // Reload enrollment with updated progress
        var updated = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();
        return MapToProgressResponse(updated);
    }

    // ─── Get Progress ─────────────────────────────────────────────────────────
    public async Task<ProgressResponse> GetProgressAsync(Guid studentId, Guid enrollmentId)
    {
        var enrollment = await _enrollmentRepo.GetByIdWithProgressAsync(enrollmentId)
            ?? throw new EnrollmentNotFoundException();

        if (enrollment.StudentId != studentId)
            throw new UnauthorizedEnrollmentAccessException();

        return MapToProgressResponse(enrollment);
    }

    // ─── Mapping Helpers ──────────────────────────────────────────────────────
    private static EnrollmentResponse MapToEnrollmentResponse(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        StudentName = e.StudentName,
        CourseId = e.CourseId,
        CourseTitle = e.CourseTitle,
        PricePaid = e.PricePaid,
        Status = e.Status.ToString(),
        EnrolledAt = e.EnrolledAt,
        CompletedAt = e.CompletedAt
    };

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

    private static ProgressResponse MapToProgressResponse(Enrollment e)
    {
        var total = e.LessonProgresses.Count;
        var completed = e.LessonProgresses.Count(p => p.IsCompleted);
        var percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 2);

        return new ProgressResponse
        {
            EnrollmentId = e.EnrollmentId,
            CourseId = e.CourseId,
            TotalLessons = total,
            CompletedLessons = completed,
            ProgressPercentage = percentage,
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