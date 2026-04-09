namespace QuizService.Domain.Entities;

public class Quiz
{
    public Guid QuizId { get; set; }
    public Guid CourseId { get; set; }

    public string Title { get; set; }
    public int PassingScore { get; set; }

    public ICollection<Question> Questions { get; set; }
}