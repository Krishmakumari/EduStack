// Create Question DTO — Input for appending a question to a quiz.

using QuizService.Domain.Enums;

namespace QuizService.Application.DTOs;

public class CreateQuestionDto
{
    public string Text { get; set; } = default!;
    
    // e.g. "MultipleChoice", "TrueFalse"
    public string Type { get; set; } = default!;
    
    // The expected correct answer. 
    public string CorrectAnswer { get; set; } = default!;
    
    // Optional JSON string containing choices (e.g. "['Delhi','Mumbai','Chennai']").
    public string? Options { get; set; }
}