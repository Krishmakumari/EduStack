namespace CertificateService.Domain.Events;

public class CertificateGeneratedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
}
