using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private Guid GetStudentId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    private string GetStudentName()
        => User.FindFirstValue("fullName") ?? "Unknown";

    // ─── POST api/enrollments ─────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(
            GetStudentId(),
            GetStudentName(),
            request);
        return Ok(result);
    }

    // ─── GET api/enrollments/my ───────────────────────────────────────────────
    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(GetStudentId());
        return Ok(result);
    }

    // ─── GET api/enrollments/{id} ─────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _enrollmentService.GetEnrollmentByIdAsync(GetStudentId(), id);
        return Ok(result);
    }

    // ─── GET api/enrollments/course/{courseId} ────────────────────────────────
    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(result);
    }

    // ─── POST api/enrollments/{id}/complete-lesson ────────────────────────────
    [HttpPost("{id:guid}/complete-lesson")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MarkLessonComplete(
        Guid id, [FromBody] MarkLessonCompleteRequest request)
    {
        var result = await _enrollmentService.MarkLessonCompleteAsync(
            GetStudentId(), id, request.LessonId);
        return Ok(result);
    }

    // ─── GET api/enrollments/{id}/progress ───────────────────────────────────
    [HttpGet("{id:guid}/progress")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetProgress(Guid id)
    {
        var result = await _enrollmentService.GetProgressAsync(GetStudentId(), id);
        return Ok(result);
    }
}