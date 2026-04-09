namespace QuizService.Domain.Entities;

public class Question
{
    public Guid QuestionId { get; set; }
    public Guid QuizId { get; set; }

    public string Text { get; set; }

    public string OptionA { get; set; }
    public string OptionB { get; set; }
    public string OptionC { get; set; }
    public string OptionD { get; set; }

    public string CorrectAnswer { get; set; }

    public Quiz Quiz { get; set; }
}