namespace EnrollmentService.Domain.Events;

public class EnrollmentCompletedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
}
