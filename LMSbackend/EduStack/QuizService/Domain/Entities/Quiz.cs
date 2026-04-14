// Quiz Entity — Represents a test shell tied to a course.
// • Contains a minimum required score (PassingScore).
// • Owns a collection of Questions (Cascade delete mapping in EF Fluent API).

namespace QuizService.Domain.Entities;

public class Quiz
{
    public Guid QuizId { get; set; }
    
    // Links this quiz to a specific course in the LMS ecosystem.
    public Guid CourseId { get; set; }
    
    public string Title { get; set; } = default!;
    
    // Score threshold for passing (e.g. 50.0). Compare against Attempt.Score.
    public decimal PassingScore { get; set; }
    
    public DateTime CreatedAt { get; set; }

    // Navigation property for all child questions.
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}