namespace CourseService.Application.DTOs.Responses;

public class CourseResponse
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
    public decimal Price { get; set; }
    public string Level { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Language { get; set; } = default!;
    public string InstructorName { get; set; } = default!;
    public Guid InstructorId { get; set; }
    public DateTime CreatedAt { get; set; }
}