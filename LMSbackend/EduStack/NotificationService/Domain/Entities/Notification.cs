using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }

    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }

    public NotificationType Type { get; set; }

    public DateTime CreatedAt { get; set; }
}