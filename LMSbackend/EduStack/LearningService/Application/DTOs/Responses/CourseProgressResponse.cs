namespace LearningService.Application.DTOs.Responses;

public class CourseProgressResponse
{
    public Guid CourseId { get; set; }

    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }

    public double CompletionPercentage { get; set; }
}