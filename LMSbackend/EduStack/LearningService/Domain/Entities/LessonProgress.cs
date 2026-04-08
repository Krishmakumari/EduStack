namespace LearningService.Domain.Entities;

public class LessonProgress
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }

    public int WatchedSeconds { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;

    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}