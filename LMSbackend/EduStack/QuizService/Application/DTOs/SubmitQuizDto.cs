// Submit Quiz DTO — Input payload for finishing an attempt.

namespace QuizService.Application.DTOs;

public class SubmitQuizDto
{
    // List of student responses mapped to question IDs.
    public List<AnswerDto> Answers { get; set; } = new();
}
