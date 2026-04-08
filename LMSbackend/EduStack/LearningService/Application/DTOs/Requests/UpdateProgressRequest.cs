namespace LearningService.Application.DTOs.Requests;

public class UpdateProgressRequest
{
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }

    public int WatchedSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
}