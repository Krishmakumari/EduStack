namespace NotificationService.Domain.Events;

public class OtpGeneratedEvent
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
