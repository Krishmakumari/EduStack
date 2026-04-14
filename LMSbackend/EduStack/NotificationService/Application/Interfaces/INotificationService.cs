// INotificationService — Contract for sending notifications (Application Layer).
// • Dependency Inversion: controller depends on this interface, not the concrete class.
// • Currently only email sending — future: could add SendSms, SendPushNotification.

using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    // Sends an email using the details in the DTO.
    // Used by the HTTP path (NotificationController).
    // The async RabbitMQ path (RabbitMqConsumer) calls EmailService directly.
    Task SendEmailAsync(SendEmailDto dto);
}
