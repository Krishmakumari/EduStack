namespace NotificationService.Domain.Events;

public class QuizResultEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public bool Passed { get; set; }
}
