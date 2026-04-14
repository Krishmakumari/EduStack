// Notification Entity — Represents a sent notification record (Domain Layer).
// • Currently DEFINED but NOT persisted — no DbContext or repository for this entity.
// • Intention: store an audit history of every email sent (who, what, when, what type).
// • Production upgrade: add NotificationDbContext + repository to save after each send.

using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }   // PK — unique per notification

    public string ToEmail { get; set; }        // recipient email address
    public string Subject { get; set; }        // what the email was about
    public string Message { get; set; }        // full email body text

    // Categorizes the notification for filtering/reporting (OTP, Enrollment, etc.)
    public NotificationType Type { get; set; }

    public DateTime CreatedAt { get; set; }    // when the notification was sent (UTC)
}