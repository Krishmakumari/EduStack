using LearningService.Application.DTOs.Requests;
using LearningService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningService.API.Controllers;

[ApiController]
[Route("api/learning")]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    // POST api/learning/progress
    [HttpPost("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        await _learningService.UpdateProgressAsync(request, GetUserId());
        return Ok(new { message = "Progress updated successfully." });
    }

    // GET api/learning/courses/{courseId}/lessons/{lessonId}/progress
    [HttpGet("courses/{courseId:guid}/lessons/{lessonId:guid}/progress")]
    public async Task<IActionResult> GetLessonProgress(Guid courseId, Guid lessonId)
    {
        var result = await _learningService.GetLessonProgressAsync(courseId, lessonId, GetUserId());
        return Ok(result);
    }

    // GET api/learning/courses/{courseId}/progress
    [HttpGet("courses/{courseId:guid}/progress")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var result = await _learningService.GetCourseProgressAsync(courseId, GetUserId());
        return Ok(result);
    }
}
