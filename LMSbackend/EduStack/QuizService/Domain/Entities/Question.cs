// Question Entity — Domain representation of a single quiz question.
// • Type is an enum (MultipleChoice, TrueFalse, etc.)
// • CorrectAnswer matches EXACTLY with student inputs (verified in service logic).
// • Options captures the MC/Checkbox choices (typically formatted as JSON array string).

using QuizService.Domain.Enums;
using System.Text.Json.Serialization;

namespace QuizService.Domain.Entities;

public class Question
{
    public Guid QuestionId { get; set; }
    
    // Parent Quiz Reference
    public Guid QuizId { get; set; }
    
    public string Text { get; set; } = default!;
    
    public QuestionType Type { get; set; }
    
    public string CorrectAnswer { get; set; } = default!;
    
    // Optional stringified JSON structure of choices.
    public string? Options { get; set; }

    [JsonIgnore]
    public Quiz Quiz { get; set; } = default!;
}