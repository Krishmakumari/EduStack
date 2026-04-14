// Answer DTO — Input for a single answer submission during a quiz attempt.

namespace QuizService.Application.DTOs;

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    
    // The textual answer selected or provided by the student.
    // Compared against Question.CorrectAnswer using case-insensitive ordinal matching.
    public string SelectedAnswer { get; set; } = default!;
}