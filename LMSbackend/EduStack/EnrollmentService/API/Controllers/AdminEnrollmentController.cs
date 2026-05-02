using EnrollmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/admin/enrollments")]
[Authorize(Roles = "Admin,Instructor")]
public class AdminEnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public AdminEnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllEnrollments()
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync();
        return Ok(result);
    }

    [HttpGet("course/{courseId:guid}")]
    public async Task<IActionResult> GetEnrollmentsByCourse(Guid courseId)
    {
        var result = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(result);
    }
}
