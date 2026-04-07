namespace EnrollmentService.Application.DTOs.Responses;

public class LessonProgressResponse
{
    public Guid LessonProgressId { get; set; }
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}