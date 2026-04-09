namespace QuizService.Application.DTOs;

public class CreateQuizDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; }
    public int PassingScore { get; set; }
}