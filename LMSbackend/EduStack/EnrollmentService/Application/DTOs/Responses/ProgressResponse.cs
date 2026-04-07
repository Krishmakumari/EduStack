namespace EnrollmentService.Application.DTOs.Responses;

public class ProgressResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public string Status { get; set; } = default!;
    public List<LessonProgressResponse> LessonProgresses { get; set; } = new();
}