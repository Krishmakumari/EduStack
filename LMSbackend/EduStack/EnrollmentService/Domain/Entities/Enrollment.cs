using EnrollmentService.Domain.Enums;

namespace EnrollmentService.Domain.Entities;

public class Enrollment
{
    public Guid EnrollmentId { get; private set; }
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; } = default!;
    public Guid CourseId { get; private set; }
    public string CourseTitle { get; private set; } = default!;
    public decimal PricePaid { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public ICollection<LessonProgress> LessonProgresses { get; private set; }
        = new List<LessonProgress>();

    private Enrollment() { }

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
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted()
    {
        Status = EnrollmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = EnrollmentStatus.Cancelled;
    }
}