namespace QuizService.Application.DTOs;

public class UpdateQuizDto
{
    public string Title { get; set; } = default!;
    public decimal PassingScore { get; set; }
}
