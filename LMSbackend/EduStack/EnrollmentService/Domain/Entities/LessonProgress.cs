namespace EnrollmentService.Domain.Entities;

public class LessonProgress
{
    public Guid LessonProgressId { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public Guid LessonId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Enrollment Enrollment { get; private set; } = default!;

    private LessonProgress() { }

    public static LessonProgress Create(Guid enrollmentId, Guid lessonId)
    {
        return new LessonProgress
        {
            LessonProgressId = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            LessonId = lessonId,
            IsCompleted = false
        };
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }
}