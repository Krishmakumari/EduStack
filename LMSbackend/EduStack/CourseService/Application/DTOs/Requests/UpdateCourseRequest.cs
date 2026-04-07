namespace CourseService.Application.DTOs.Requests;

public class UpdateCourseRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
    public decimal Price { get; set; }
    public string Level { get; set; } = default!;
    public string Language { get; set; } = default!;
}