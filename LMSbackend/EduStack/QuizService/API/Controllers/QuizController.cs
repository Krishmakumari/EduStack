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
    private Guid GetStudentId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Student ID claim missing."));

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

    // POST api/quizzes/{id}/questions
    // Appends a single question to an existing quiz.
    [HttpPost("{id}/questions")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> AddQuestion(Guid id, [FromBody] CreateQuestionDto dto)
    {
        await _quizService.AddQuestionAsync(id, dto);
        return Ok("Question added successfully.");
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
        var result = await _quizService.SubmitQuizAsync(attemptId, dto, GetStudentId());
        return Ok(result);
    }
}