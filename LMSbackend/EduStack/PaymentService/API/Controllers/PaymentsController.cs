using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.Interfaces;
using System.Security.Claims;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Guid GetStudentId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Student ID claim missing."));

    private string GetStudentName() =>
        User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("name")
            ?? "Unknown";

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
    {
        var result = await _paymentService.InitiatePaymentAsync(GetStudentId(), GetStudentName(), request);
        return Ok(result);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        var result = await _paymentService.ConfirmPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    [HttpPost("{id}/fail")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailPaymentRequest request)
    {
        var result = await _paymentService.FailPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentRequest request)
    {
        var result = await _paymentService.RefundPaymentAsync(GetStudentId(), id, request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _paymentService.GetMyPaymentsAsync(GetStudentId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(GetStudentId(), id);
        return Ok(result);
    }

    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var result = await _paymentService.GetPaymentsByCourseAsync(courseId);
        return Ok(result);
    }
}
