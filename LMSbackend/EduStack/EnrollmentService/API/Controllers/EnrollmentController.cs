// EnrollmentController — HTTP entry point for all enrollment and progress operations.
// • ALL endpoints require authentication ([Authorize] on the class).
// • Student role: enroll, view own courses, mark lessons complete, check progress.
// • Instructor/Admin role: view all students enrolled in a course.
// • studentId extracted from JWT claims — ensures students only see their own data.

using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]    // ← all endpoints require a valid JWT — no anonymous access
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // ─── JWT Claim Helpers ────────────────────────────────────────────────────

    // Extracts the student's unique ID from the JWT token.
    // Tries ClaimTypes.NameIdentifier first (standard), then "sub" (custom).
    // Throws UnauthorizedAccessException if the claim is missing (shouldn't happen
    // if [Authorize] is working, but defensive coding is good practice).
    private Guid GetStudentId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }

    // Extracts the student's display name from the JWT "fullName" claim.
    // Denormalized onto the Enrollment entity — no Auth Service call needed.
    private string GetStudentName()
        => User.FindFirstValue("fullName") ?? "Unknown";

    private string GetStudentEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "Unknown";

    // ─── Enrollment Endpoints ─────────────────────────────────────────────────

    // POST api/enrollments — Student only.
    // Enrolls the currently logged-in student in a course.
    // StudentId and StudentName come from JWT claims (not the request body).
    // Returns 400 if already enrolled, 200 with enrollment details on success.
    [HttpPost]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(
            GetStudentId(),     // from JWT "sub" claim
            GetStudentName(),   // from JWT "fullName" claim
            GetStudentEmail(),  // from JWT "email" claim
            request);
        return Ok(result);
    }

    // GET api/enrollments/my — Student, Instructor, Admin.
    // Returns all courses the user is enrolled in (lightweight list, no lesson data).
    // The "my" prefix scopes to the JWT user — no user can see another's list.
    [HttpGet("my")]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(GetStudentId());
        return Ok(result);
    }

    // GET api/enrollments/{id} — Student only.
    // Returns full enrollment detail including all lesson progress records.
    // Service validates the enrollment belongs to the JWT student (ownership check).
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _enrollmentService.GetEnrollmentByIdAsync(GetStudentId(), id);
        return Ok(result);
    }

    // GET api/enrollments/course/{courseId} — Instructor/Admin only.
    // Returns all students enrolled in a specific course.
    // Used for instructor dashboards to see enrollment counts and student lists.
    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(result);
    }

    // ─── Progress Endpoints ───────────────────────────────────────────────────

    // POST api/enrollments/{id}/complete-lesson — Student only.
    // Marks a specific lesson as complete within an enrollment.
    // Request body: { "lessonId": "guid" }
    // Service handles upsert logic and prevents double-completion.
    [HttpPost("{id:guid}/complete-lesson")]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> MarkLessonComplete(
        Guid id, [FromBody] MarkLessonCompleteRequest request)
    {
        var result = await _enrollmentService.MarkLessonCompleteAsync(
            GetStudentId(), id, request.LessonId);
        return Ok(result);
    }

    // GET api/enrollments/{id}/progress — Student only.
    // Returns progress stats: total lessons, completed count, and % complete.
    // Ownership validated in service layer — student can only see their own progress.
    [HttpGet("{id:guid}/progress")]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> GetProgress(Guid id)
    {
        var result = await _enrollmentService.GetProgressAsync(GetStudentId(), id);
        return Ok(result);
    }

    // POST api/enrollments/{id}/sync-total — Student only.
    // Repairs the total lessons count for an existing enrollment.
    [HttpPost("{id:guid}/sync-total")]
    [Authorize(Roles = "Student,Instructor,Admin")]
    public async Task<IActionResult> SyncTotalLessons(Guid id, [FromBody] SyncTotalRequest request)
    {
        await _enrollmentService.SyncTotalLessonsAsync(GetStudentId(), id, request.TotalLessons);
        return Ok(new { message = "Total lessons synced successfully." });
    }

    // ─── Internal Cross-Service Endpoints ─────────────────────────────────────

    // GET api/enrollments/check?userId=guid&courseId=guid
    [HttpGet("check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEnrollment([FromQuery] Guid userId, [FromQuery] Guid courseId)
    {
        var result = await _enrollmentService.IsUserEnrolledAsync(userId, courseId);
        return Ok(result);
    }
}