// QuizAttempt Entity — Tracks a single student's run through a quiz.
// • Stores the attempt status lifecycle: InProgress → Passed/Failed.
// • Maintains the calculated final Score out of 100.
// • Owns a collection of UserAnswers tracking every individual choice made.

using QuizService.Domain.Enums;
using System.Text.Json.Serialization;

namespace QuizService.Domain.Entities;

public class QuizAttempt
{
    public Guid AttemptId { get; set; }
    
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    
    // Derived value (CorrectAnswers * 100 / TotalQuestions).
    public decimal Score { get; set; }
    
    // InProgress (when taking), Passed or Failed (when submitted).
    public AttemptStatus Status { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [JsonIgnore]
    public Quiz Quiz { get; set; } = default!;
    
    public ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
}