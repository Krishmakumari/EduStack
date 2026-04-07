namespace CourseService.Application.DTOs.Requests;

public class AddSectionRequest
{
    public string Title { get; set; } = default!;
    public int Order { get; set; }
}