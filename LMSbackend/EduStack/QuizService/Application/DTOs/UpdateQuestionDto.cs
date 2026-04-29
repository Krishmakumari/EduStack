namespace QuizService.Application.DTOs;

public class UpdateQuestionDto
{
    public string Text { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string CorrectAnswer { get; set; } = default!;
    public string? Options { get; set; }
}
