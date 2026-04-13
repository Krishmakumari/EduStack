using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(SendEmailDto dto);
}
