// PaymentsController — Student-facing HTTP endpoints for payment operations.
// • ALL endpoints require JWT auth ([Authorize] on class).
// • Most operations: student can only manage their own payments (ownership validated in service).
// • Exception: GET course/{courseId} is restricted to Admin/Instructor only.
// • StudentId from JWT "sub" claim; StudentName from JWT "name" claim.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.Interfaces;
using System.Security.Claims;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]    // all endpoints require a valid JWT token
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // Extracts student GUID from the JWT "sub" / NameIdentifier claim.
    private Guid GetStudentId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Student ID claim missing."));

    // Extracts student display name from JWT "name" claim (denormalized onto Payment entity).
    private string GetStudentName() =>
        User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("name")
            ?? "Unknown";

    // ─── Lifecycle Endpoints ──────────────────────────────────────────────────

    // POST api/payments/initiate — Start a new payment (Pending status).
    // StudentId and StudentName come from JWT — not from the request body.
    // Returns 400 if already purchased or invalid method string.
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
    {
        var result = await _paymentService.InitiatePaymentAsync(GetStudentId(), GetStudentName(), request);
        return Ok(result);
    }

    // POST api/payments/{id}/confirm — Mark payment as Completed.
    // Called after the payment gateway confirms the transaction.
    // Body: { "transactionId": "gateway-ref-123" }
    // Returns 400 if already completed, 403 if wrong student.
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        var result = await _paymentService.ConfirmPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    // POST api/payments/{id}/fail — Mark payment as Failed.
    // Called when the payment gateway reports failure (declined card, timeout, etc.).
    // Body: { "reason": "Insufficient funds" }
    [HttpPost("{id}/fail")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailPaymentRequest request)
    {
        var result = await _paymentService.FailPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    // POST api/payments/{id}/refund — Request a refund for a completed payment.
    // Creates an associated Refund entity and moves payment to Refunded status.
    // Body: { "reason": "Course not as described" }
    // Returns 400 if payment is not in Completed state (guard in domain entity).
    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentRequest request)
    {
        var result = await _paymentService.RefundPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    // ─── Query Endpoints ──────────────────────────────────────────────────────

    // GET api/payments/my — Student's full purchase history.
    // Scoped to the JWT student — no student can see another's payment history.
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _paymentService.GetMyPaymentsAsync(GetStudentId());
        return Ok(result);
    }

    // GET api/payments/{id} — Single payment detail with ownership validation.
    // Returns 404 if not found, 403 if wrong student.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(GetStudentId(), id);
        return Ok(result);
    }

    // GET api/payments/course/{courseId} — All payments for a course.
    // Role-restricted: Admin and Instructor only — for revenue dashboards.
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _paymentService.GetPaymentsByCourseAsync(courseId);
        return Ok(result);
    }
}
