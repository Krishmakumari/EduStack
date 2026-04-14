// UserAnswer Entity — Records a student's answer to a specific question.
// • Linked to the QuizAttempt (parent container for the session).
// • Caches whether the answer was correct (IsCorrect flag), calculated at submission.

using System.Text.Json.Serialization;

namespace QuizService.Domain.Entities;

public class UserAnswer
{
    public Guid AnswerId { get; set; }
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    
    // The exact string the student selected/typed.
    public string SelectedAnswer { get; set; } = default!;
    
    // Precalculated during grading to avoid recalculations if the Question definition changes later.
    public bool IsCorrect { get; set; }

    [JsonIgnore]
    public QuizAttempt Attempt { get; set; } = default!;
}