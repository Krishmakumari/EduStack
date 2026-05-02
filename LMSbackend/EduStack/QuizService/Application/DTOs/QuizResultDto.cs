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

    // Detailed results for each question to help with debugging.
    public List<QuestionResultDto> Details { get; set; } = new();
}

public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}