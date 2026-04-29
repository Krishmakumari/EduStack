namespace QuizService.Domain.Events;

public class QuizResultEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Passed { get; set; }
}
