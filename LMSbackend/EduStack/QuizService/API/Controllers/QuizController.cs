// Quiz Controller — Endpoints for managing and taking quizzes.
// • Admin/Instructor operations: create quiz, add questions (requires roles).
// • Student operations: start quiz, submit quiz (secured, scoped to JWT user).
// • Returns clean DTOs; errors handled globally by ExceptionMiddleware.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizService.Application.DTOs;
using QuizService.Application.Interfaces;
using System.Security.Claims;

namespace QuizService.API.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize] // all endpoints require valid JWT authentication
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    // Helper to extract student ID securely from JWT.
    private Guid GetStudentId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirstValue("sub") 
                     ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User ID claim missing in token.");

        return Guid.Parse(idClaim);
    }

    private string GetStudentEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "Unknown";

    // ─── Admin / Instructor Endpoints ────────────────────────────────────────

    // POST api/quizzes
    // Creates a new quiz shell attached to a specific course.
    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        var quizId = await _quizService.CreateQuizAsync(dto);
        return CreatedAtAction(nameof(GetQuiz), new { id = quizId }, new { QuizId = quizId });
    }

    // PUT api/quizzes/{id}
    // Updates basic quiz details (Title, PassingScore).
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] UpdateQuizDto dto)
    {
        await _quizService.UpdateQuizAsync(id, dto);
        return Ok(new { message = "Quiz updated successfully." });
    }

    // POST api/quizzes/{id}/questions
    // Appends a single question to an existing quiz.
    [HttpPost("{id}/questions")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> AddQuestion(Guid id, [FromBody] CreateQuestionDto dto)
    {
        await _quizService.AddQuestionAsync(id, dto);
        return Ok(new { message = "Question added successfully." });
    }

    // PUT api/quizzes/questions/{questionId}
    [HttpPut("questions/{questionId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionDto dto)
    {
        await _quizService.UpdateQuestionAsync(questionId, dto);
        return Ok(new { message = "Question updated successfully." });
    }

    // DELETE api/quizzes/questions/{questionId}
    [HttpDelete("questions/{questionId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId)
    {
        await _quizService.DeleteQuestionAsync(questionId);
        return Ok(new { message = "Question deleted successfully." });
    }

    // ─── Shared Endpoints ───────────────────────────────────────────────────

    // GET api/quizzes/{id}
    // Retrieves quiz details and all questions.
    // In production, you'd hide CorrectAnswer for students, but this is a simplified version.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(Guid id)
    {
        var quiz = await _quizService.GetQuizDetailsAsync(id);
        return Ok(quiz);
    }

    // GET api/quizzes/course/{courseId}
    // Retrieves quiz for a specific course
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetQuizByCourse(Guid courseId)
    {
        var quiz = await _quizService.GetQuizByCourseIdAsync(courseId);
        if (quiz == null) return NotFound("Quiz not found for this course.");
        return Ok(quiz);
    }

    // ─── Student Endpoints ──────────────────────────────────────────────────

    // POST api/quizzes/{id}/start
    // Begins a quiz attempt, returning an AttemptId. Sets status to InProgress.
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartQuiz(Guid id)
    {
        var attemptId = await _quizService.StartQuizAsync(id, GetStudentId());
        return Ok(new { AttemptId = attemptId });
    }

    // POST api/quizzes/attempt/{attemptId}/submit
    // Grades the attempt. Calculates score, determines pass/fail, and marks CompletedAt.
    // Validates ownership: only the student who started the attempt can submit it.
    [HttpPost("attempt/{attemptId}/submit")]
    public async Task<IActionResult> SubmitQuiz(Guid attemptId, [FromBody] SubmitQuizDto dto)
    {
        var result = await _quizService.SubmitQuizAsync(attemptId, dto, GetStudentId(), GetStudentEmail());
        return Ok(result);
    }
}