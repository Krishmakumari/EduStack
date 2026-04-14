// CertificateGeneratedEvent — RabbitMQ message contract for the certificate_queue.
// • Published by CertificateService when a PDF certificate is generated.
// • Contains Email directly (event-carried state transfer) — Notification Service
//   doesn't need to call Auth Service to look up the student's email.

namespace NotificationService.Domain.Events;

public class CertificateGeneratedEvent
{
    public Guid UserId { get; set; }       // student who earned the certificate
    public string Email { get; set; }      // where to send the notification
    public string CourseTitle { get; set; } // included in the email body
}