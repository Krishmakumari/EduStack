// Enrollment Entity — Tracks a student's registration in a course (Domain Layer).
// • DDD design: private constructor + factory method guarantees valid initial state.
// • Private setters: all state changes go through domain methods (MarkCompleted, Cancel).
// • Denormalized StudentName + CourseTitle: captured at enrollment time for history/display.
// • LessonProgresses: navigation collection (lazy create — no pre-seeded records).

using EnrollmentService.Domain.Enums;

namespace EnrollmentService.Domain.Entities;

public class Enrollment
{
    public Guid EnrollmentId { get; private set; }    // PK

    public Guid StudentId { get; private set; }        // from JWT "sub" claim
    public string StudentName { get; private set; } = default!; // denormalized from JWT

    public Guid CourseId { get; private set; }         // from Course Service
    public string CourseTitle { get; private set; } = default!; // denormalized from request

    public decimal PricePaid { get; private set; }     // amount paid at time of enrollment

    // Status lifecycle: Active → Completed (when all lessons done) OR Cancelled.
    public EnrollmentStatus Status { get; private set; }

    public DateTime EnrolledAt { get; private set; }   // when the student enrolled (UTC)
    public DateTime? CompletedAt { get; private set; } // null until course is completed

    // Navigation property — loaded via .Include() in GetByIdWithProgressAsync.
    // Created lazily as lessons are marked complete (no pre-created "pending" records).
    public ICollection<LessonProgress> LessonProgresses { get; private set; }
        = new List<LessonProgress>();

    // Private constructor — prevents construction outside of this class.
    // Forces all callers to use the Create() factory method.
    private Enrollment() { }

    // Factory method (DDD pattern) — the only way to create a valid Enrollment.
    // Sets Status = Active and EnrolledAt = UtcNow automatically.
    // No "half-baked" enrollments can be created bypassing this.
    public static Enrollment Create(
        Guid studentId,
        string studentName,
        Guid courseId,
        string courseTitle,
        decimal pricePaid)
    {
        return new Enrollment
        {
            EnrollmentId = Guid.NewGuid(),
            StudentId = studentId,
            StudentName = studentName,
            CourseId = courseId,
            CourseTitle = courseTitle,
            PricePaid = pricePaid,
            Status = EnrollmentStatus.Active,  // always starts Active
            EnrolledAt = DateTime.UtcNow       // captured at creation time
        };
    }

    // MarkCompleted — transitions enrollment to Completed state.
    // Records the exact time the student finished the course.
    public void MarkCompleted()
    {
        Status = EnrollmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    // Cancel — transitions enrollment to Cancelled state (e.g. refund/withdrawal).
    public void Cancel()
    {
        Status = EnrollmentStatus.Cancelled;
    }
}