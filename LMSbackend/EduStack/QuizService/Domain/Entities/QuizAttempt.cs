using QuizService.Domain.Enums;

namespace QuizService.Domain.Entities;

public class QuizAttempt
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public Guid UserId { get; set; }

    public int Score { get; set; }
    public bool IsPassed { get; set; }

    public DateTime AttemptedAt { get; set; }
    public AttemptStatus Status { get; set; }
}