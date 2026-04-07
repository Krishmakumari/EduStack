namespace EnrollmentService.Application.DTOs.Requests;

public class EnrollRequest
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public decimal PricePaid { get; set; }
}