namespace LearningService.Application.DTOs.Responses;

public class LessonProgressResponse
{
    public Guid LessonId { get; set; }

    public int WatchedSeconds { get; set; }
    public bool IsCompleted { get; set; }

    public double ProgressPercentage { get; set; }
}