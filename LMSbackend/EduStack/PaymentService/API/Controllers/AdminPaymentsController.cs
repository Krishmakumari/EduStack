using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin,Instructor")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetPaymentsByCourse(Guid courseId)
    {
        var result = await _paymentService.GetPaymentsByCourseAsync(courseId);
        return Ok(result);
    }
}