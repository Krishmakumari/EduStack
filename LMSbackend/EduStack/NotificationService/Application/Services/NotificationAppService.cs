// NotificationAppService — Application facade for the direct HTTP notification path.
// • Thin layer between controller and EmailService — delegates immediately.
// • WHY this layer exists: Dependency Inversion — controller depends on INotificationService,
//   not EmailService directly. Makes it easy to swap email provider or add logic (logging, etc.).

using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Application.Services;

public class NotificationAppService : INotificationService
{
    // EmailService is the actual SMTP sender — injected here.
    private readonly EmailService _emailService;

    public NotificationAppService(EmailService emailService)
    {
        _emailService = emailService;
    }

    // SendEmailAsync — passes the DTO fields directly to EmailService.
    // Any future logic (logging, saving to DB, notification history) would go here.
    public async Task SendEmailAsync(SendEmailDto dto)
    {
        await _emailService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Message);
    }
}
