namespace CourseService.Application.DTOs.Requests;

public class UpdateLessonRequest
{
    public string Title { get; set; } = default!;
    public string? VideoUrl { get; set; }                   
    public string? Content { get; set; }
    public int DurationInSeconds { get; set; }
    public int Order { get; set; }
    public bool IsFreePreview { get; set; }
}