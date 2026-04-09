namespace QuizService.Application.DTOs;

public class SubmitQuizDto
{
    public Guid QuizId { get; set; }
    public Guid UserId { get; set; }

    public List<AnswerDto> Answers { get; set; }
}
