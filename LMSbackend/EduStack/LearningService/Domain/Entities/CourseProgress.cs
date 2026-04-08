namespace LearningService.Domain.Entities;

public class CourseProgress
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }

    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }

    public double CompletionPercentage =>
        TotalLessons == 0 ? 0 : (double)CompletedLessons / TotalLessons * 100;
}