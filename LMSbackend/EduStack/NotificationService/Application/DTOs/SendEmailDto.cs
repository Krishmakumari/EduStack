// SendEmailDto — Input DTO for the direct HTTP email sending endpoint.
// • Used by POST /api/notification/send-email.
// • Simple flat structure — caller provides all fields explicitly.

namespace NotificationService.Application.DTOs;

public class SendEmailDto
{
    public string ToEmail { get; set; } = string.Empty;   // recipient email address
    public string Subject { get; set; } = string.Empty;   // email subject line
    public string Message { get; set; } = string.Empty;   // email body (plain text)
}
