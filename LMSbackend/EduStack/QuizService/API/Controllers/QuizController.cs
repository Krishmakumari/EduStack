using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizService.Application.DTOs;
using QuizService.Application.Interfaces;

namespace QuizService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    // 👉 Create Quiz (Instructor)
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateQuiz(CreateQuizDto dto)
    {
        var quizId = await _quizService.CreateQuizAsync(dto);
        return Ok(quizId);
    }

    // 👉 Add Question
    [HttpPost("question")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> AddQuestion(CreateQuestionDto dto)
    {
        await _quizService.AddQuestionAsync(dto);
        return Ok("Question added");
    }

    // 👉 Submit Quiz (Student)
    [HttpPost("submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitQuiz(SubmitQuizDto dto)
    {
        var result = await _quizService.SubmitQuizAsync(dto);
        return Ok(result);
    }
}