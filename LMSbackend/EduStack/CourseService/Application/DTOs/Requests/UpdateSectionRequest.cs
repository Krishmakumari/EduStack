namespace CourseService.Application.DTOs.Requests;

public class UpdateSectionRequest
{
    public string Title { get; set; } = default!;
    public int Order { get; set; }
}