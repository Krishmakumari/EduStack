// AdminPaymentsController — Admin/Instructor-only endpoints for payment reporting.
// • Separate controller from PaymentsController — clean role separation at HTTP level.
// • [Authorize(Roles="Admin,Instructor")] on class — all endpoints require elevated role.
// • Currently one endpoint: view all payments for a course (revenue dashboard).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin,Instructor")]   // elevated role required for all admin endpoints
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // GET api/admin/payments/course/{courseId}
    // Returns all student payments for a specific course.
    // Used by instructors/admins to view revenue and enrollment purchase data.
    // No ownership check — admin can see ALL students' payments for any course.
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetPaymentsByCourse(Guid courseId)
    {
        var result = await _paymentService.GetPaymentsByCourseAsync(courseId);
        return Ok(result);
    }

    // GET api/admin/payments
    // Returns all payments across the system. Admin only.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPayments()
    {
        var result = await _paymentService.GetAllPaymentsAsync();
        return Ok(result);
    }
}