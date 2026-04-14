// LearningController — HTTP entry point for video progress tracking.
// • ALL endpoints require authentication ([Authorize] on class).
// • No role restrictions beyond auth — any enrolled student can call all endpoints.
// • Access control is enforced by the SERVICE layer (enrollment check via EnrollmentClient).
// • userId extracted from JWT claims — students only see their own progress.

using LearningService.Application.DTOs.Requests;
using LearningService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningService.API.Controllers;

[ApiController]
[Route("api/learning")]
[Authorize]    // all endpoints require a valid JWT token
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    // Extracts the user's unique ID from the JWT "sub" / NameIdentifier claim.
    // Passed into every service method so each student's progress is isolated.
    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    // ─── Progress Endpoints ───────────────────────────────────────────────────

    // POST api/learning/progress
    // Updates how many seconds the student has watched for a specific lesson.
    // Called frequently by the video player — typically every 30 seconds.
    // Returns 403 if not enrolled, 200 OK on success.
    [HttpPost("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        await _learningService.UpdateProgressAsync(request, GetUserId());
        return Ok(new { message = "Progress updated successfully." });
    }

    // GET api/learning/courses/{courseId}/lessons/{lessonId}/progress
    // Returns progress for ONE specific lesson: watchedSeconds, isCompleted.
    // Used by the video player to resume from the correct position.
    // Returns 404 if the student hasn't started this lesson yet.
    [HttpGet("courses/{courseId:guid}/lessons/{lessonId:guid}/progress")]
    public async Task<IActionResult> GetLessonProgress(Guid courseId, Guid lessonId)
    {
        var result = await _learningService.GetLessonProgressAsync(courseId, lessonId, GetUserId());
        return Ok(result);
    }

    // GET api/learning/courses/{courseId}/progress
    // Returns overall course progress: totalLessons, completedLessons, completionPercentage.
    // Calculated in memory from all LessonProgress records for this user + course.
    [HttpGet("courses/{courseId:guid}/progress")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var result = await _learningService.GetCourseProgressAsync(courseId, GetUserId());
        return Ok(result);
    }
}
