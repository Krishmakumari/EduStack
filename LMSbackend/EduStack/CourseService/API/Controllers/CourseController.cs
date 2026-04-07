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

    // Helpers
    private Guid GetInstructorId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    private string GetInstructorName()
        => User.FindFirstValue("fullName") ?? "Unknown";

    // GET api/courses 
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _courseService.GetAllCoursesAsync();
        return Ok(result);
    }

    // GET api/courses/my 
    [HttpGet("my")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> GetMyCourses()
    {
        var result = await _courseService.GetMyCourseAsync(GetInstructorId());
        return Ok(result);
    }

    // GET api/courses/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        return Ok(result);
    }

    // POST api/courses
    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateCourseAsync(
            GetInstructorId(),
            GetInstructorName(),
            request);
        return Ok(result);
    }

    // PUT api/courses/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var result = await _courseService.UpdateCourseAsync(GetInstructorId(), id, request);
        return Ok(result);
    }

    // DELETE api/courses/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _courseService.DeleteCourseAsync(GetInstructorId(), id);
        return Ok(new { message = "Course deleted successfully." });
    }

    // POST api/courses/{id}/publish
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _courseService.PublishCourseAsync(GetInstructorId(), id);
        return Ok(new { message = "Course published successfully." });
    }

    // POST api/courses/{id}/unpublish
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        await _courseService.UnpublishCourseAsync(GetInstructorId(), id);
        return Ok(new { message = "Course unpublished successfully." });
    }

    // POST api/courses/{id}/sections 
    [HttpPost("{id:guid}/sections")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> AddSection(Guid id, [FromBody] AddSectionRequest request)
    {
        var result = await _courseService.AddSectionAsync(GetInstructorId(), id, request);
        return Ok(result);
    }

    // PUT api/courses/sections/{sectionId}
    [HttpPut("sections/{sectionId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateSectionRequest request)
    {
        var result = await _courseService.UpdateSectionAsync(GetInstructorId(), sectionId, request);
        return Ok(result);
    }

    // DELETE api/courses/sections/{sectionId}
    [HttpDelete("sections/{sectionId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> DeleteSection(Guid sectionId)
    {
        await _courseService.DeleteSectionAsync(GetInstructorId(), sectionId);
        return Ok(new { message = "Section deleted successfully." });
    }

    // POST api/courses/sections/{sectionId}/lessons 
    [HttpPost("sections/{sectionId:guid}/lessons")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> AddLesson(Guid sectionId, [FromBody] AddLessonRequest request)
    {
        var result = await _courseService.AddLessonAsync(GetInstructorId(), sectionId, request);
        return Ok(result);
    }

    // PUT api/courses/lessons/{lessonId}
    [HttpPut("lessons/{lessonId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] UpdateLessonRequest request)
    {
        var result = await _courseService.UpdateLessonAsync(GetInstructorId(), lessonId, request);
        return Ok(result);
    }

    // DELETE api/courses/lessons/{lessonId}
    [HttpDelete("lessons/{lessonId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> DeleteLesson(Guid lessonId)
    {
        await _courseService.DeleteLessonAsync(GetInstructorId(), lessonId);
        return Ok(new { message = "Lesson deleted successfully." });
    }
}