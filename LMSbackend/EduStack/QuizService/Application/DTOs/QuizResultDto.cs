// Quiz Result DTO — Output returned upon successful submission of a quiz attempt.

namespace QuizService.Application.DTOs;

public class QuizResultDto
{
    public Guid AttemptId { get; set; }
    
    // Total calculated score (Correct / Total * 100).
    public decimal Score { get; set; }
    
    // Whether the Score >= Quiz.PassingScore.
    public bool Passed { get; set; }
    
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
}