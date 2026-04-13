using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Application.Services;

public class NotificationAppService : INotificationService
{
    private readonly EmailService _emailService;

    public NotificationAppService(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendEmailAsync(SendEmailDto dto)
    {
        await _emailService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Message);
    }
}
