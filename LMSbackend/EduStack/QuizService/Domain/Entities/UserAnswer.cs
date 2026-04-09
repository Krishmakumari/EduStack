namespace QuizService.Domain.Entities;

public class UserAnswer
{
    public Guid Id { get; set; }
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }

    public string SelectedAnswer { get; set; }
}