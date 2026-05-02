// CourseController — HTTP entry point for all course, section, and lesson operations.
// • 15 endpoints: CRUD for courses/sections/lessons + publish/unpublish.
// • Write operations require [Authorize(Roles = "Instructor,Admin")].
// • Reads instructor identity from JWT claims (no DB call needed).

using CourseService.Application.DTOs.Requests;
using CourseService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    // Extract instructor identity from JWT claims set by Auth Service.
    // "sub" claim = UserId, "fullName" = custom claim.

    private Guid GetInstructorId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    private string GetInstructorName()
        => User.FindFirstValue("fullName") ?? "Unknown";

    // ─── Course Endpoints ────────────────────────────────────────────────────

    // GET api/courses — PUBLIC, no auth needed.
    // Returns all courses for the catalog page (lightweight, no nested data).
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _courseService.GetAllCoursesAsync();
        return Ok(result);
    }

    // GET api/courses/my — Instructor/Admin only.
    // Returns only courses created by the currently logged-in instructor.
    // Used for the "My Courses" dashboard page.
    [HttpGet("my")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> GetMyCourses()
    {
        var result = await _courseService.GetMyCourseAsync(GetInstructorId());
        return Ok(result);
    }

    // GET api/courses/{id} — PUBLIC, no auth needed.
    // Returns the FULL course tree (sections + lessons nested).
    // This is the "course detail page" endpoint.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        return Ok(result);
    }

    // POST api/courses — Instructor/Admin only.
    // Creates a new course in Draft status. The instructor's ID and name
    // are extracted from JWT claims and stored on the course.
    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateCourseAsync(
            GetInstructorId(),      // from JWT "sub" claim
            GetInstructorName(),    // from JWT "fullName" claim
            request);
        return Ok(result);
    }

    // PUT api/courses/{id} — Instructor/Admin only.
    // Updates course details. Service layer checks ownership.
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var result = await _courseService.UpdateCourseAsync(GetInstructorId(), id, request);
        return Ok(result);
    }

    // DELETE api/courses/{id} — Instructor/Admin only.
    // Deletes the course + all sections + all lessons (cascade delete).
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _courseService.DeleteCourseAsync(GetInstructorId(), id);
        return Ok(new { message = "Course deleted successfully." });
    }

    // ─── Publishing Endpoints ────────────────────────────────────────────────

    // POST api/courses/{id}/publish — Instructor/Admin only.
    // Makes the course visible to students. Must have ≥1 section.
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Submit(Guid id)
    {
        await _courseService.SubmitCourseForReviewAsync(GetInstructorId(), id);
        return Ok(new { message = "Course submitted for review successfully." });
    }

    // POST api/courses/{id}/unpublish — Instructor/Admin only.
    // Reverts course to Draft. Students can no longer see it.
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        await _courseService.UnpublishCourseAsync(GetInstructorId(), id);
        return Ok(new { message = "Course unpublished successfully." });
    }

    // ─── Section Endpoints ───────────────────────────────────────────────────

    // POST api/courses/{id}/sections — Instructor/Admin only.
    // Adds a new section to a course. Ownership validated in service layer.
    [HttpPost("{id:guid}/sections")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> AddSection(Guid id, [FromBody] AddSectionRequest request)
    {
        var result = await _courseService.AddSectionAsync(GetInstructorId(), id, request);
        return Ok(result);
    }

    // PUT api/courses/sections/{sectionId} — Instructor/Admin only.
    // Updates section title and order. Service walks up to Course for ownership.
    [HttpPut("sections/{sectionId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateSectionRequest request)
    {
        var result = await _courseService.UpdateSectionAsync(GetInstructorId(), sectionId, request);
        return Ok(result);
    }

    // DELETE api/courses/sections/{sectionId} — Instructor/Admin only.
    // Deletes section + all its lessons (cascade).
    [HttpDelete("sections/{sectionId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> DeleteSection(Guid sectionId)
    {
        await _courseService.DeleteSectionAsync(GetInstructorId(), sectionId);
        return Ok(new { message = "Section deleted successfully." });
    }

    // ─── Lesson Endpoints ────────────────────────────────────────────────────

    // POST api/courses/sections/{sectionId}/lessons — Instructor/Admin only.
    // Adds a lesson (video/text content) to a section. Ownership validated two levels up.
    [HttpPost("sections/{sectionId:guid}/lessons")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> AddLesson(Guid sectionId, [FromBody] AddLessonRequest request)
    {
        var result = await _courseService.AddLessonAsync(GetInstructorId(), sectionId, request);
        return Ok(result);
    }

    // PUT api/courses/lessons/{lessonId} — Instructor/Admin only.
    // Updates lesson content, duration, order, free preview flag.
    [HttpPut("lessons/{lessonId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] UpdateLessonRequest request)
    {
        var result = await _courseService.UpdateLessonAsync(GetInstructorId(), lessonId, request);
        return Ok(result);
    }

    // DELETE api/courses/lessons/{lessonId} — Instructor/Admin only.
    // Deletes a single lesson.
    [HttpDelete("lessons/{lessonId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> DeleteLesson(Guid lessonId)
    {
        await _courseService.DeleteLessonAsync(GetInstructorId(), lessonId);
        return Ok(new { message = "Lesson deleted successfully." });
    }

}