namespace EnrollmentService.Application.DTOs.Responses;

public class EnrollmentDetailResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public decimal PricePaid { get; set; }
    public string Status { get; set; } = default!;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<LessonProgressResponse> LessonProgresses { get; set; } = new();
}