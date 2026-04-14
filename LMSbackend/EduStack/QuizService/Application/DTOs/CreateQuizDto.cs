// Create Quiz DTO — Input for initializing a new quiz for a course.

namespace QuizService.Application.DTOs;

public class CreateQuizDto
{
    public Guid CourseId { get; set; }
    
    public string Title { get; set; } = default!;
    
    // Scale of 0-100 indicating the minimum score to pass.
    public decimal PassingScore { get; set; }
}