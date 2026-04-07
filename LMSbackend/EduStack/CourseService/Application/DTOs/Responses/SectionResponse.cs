namespace CourseService.Application.DTOs.Responses;

public class SectionResponse
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = default!;
    public int Order { get; set; }
    public List<LessonResponse> Lessons { get; set; } = new();
}