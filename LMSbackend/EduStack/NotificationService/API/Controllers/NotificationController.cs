// NotificationController — HTTP entry point for direct email sending.
// • One endpoint: POST /api/notification/send-email
// • No [Authorize] — intended for internal service-to-service calls or admin use.
// • Thin controller: delegates all work to INotificationService.
// • Second notification path: the async path goes through RabbitMqConsumer (no HTTP).

using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // POST api/notification/send-email
    // Sends an email directly via HTTP request (synchronous path).
    // Used by admin or other services that need to trigger a notification immediately
    // without publishing to RabbitMQ (e.g., password reset, manual alerts).
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
    {
        await _notificationService.SendEmailAsync(dto);
        return Ok("Email sent successfully");
    }
}
