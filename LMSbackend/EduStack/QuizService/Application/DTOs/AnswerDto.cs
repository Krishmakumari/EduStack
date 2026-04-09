namespace QuizService.Application.DTOs;

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public string SelectedAnswer { get; set; }
}