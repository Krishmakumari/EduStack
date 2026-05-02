using CourseService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize(Roles = "Admin")]
public class AdminCourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public AdminCourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        var result = await _courseService.GetAllCoursesAdminAsync();
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveCourse(Guid id)
    {
        await _courseService.ApproveCourseAsync(id);
        return Ok(new { message = "Course approved and published successfully." });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectCourse(Guid id)
    {
        await _courseService.RejectCourseAsync(id);
        return Ok(new { message = "Course rejected." });
    }

    [HttpDelete("{courseId:guid}")]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        await _courseService.AdminDeleteCourseAsync(courseId);
        return Ok(new { message = "Course deleted by Admin successfully." });
    }
}
