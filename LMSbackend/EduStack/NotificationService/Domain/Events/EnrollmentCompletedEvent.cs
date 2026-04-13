namespace NotificationService.Domain.Events;

public class EnrollmentCompletedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string CourseTitle { get; set; }
}
