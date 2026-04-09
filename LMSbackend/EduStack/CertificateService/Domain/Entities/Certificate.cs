namespace CertificateService.Domain.Entities;

public class Certificate
{
    public Guid CertificateId { get; set; }

    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    public string UserName { get; set; }
    public string CourseTitle { get; set; }

    public DateTime IssuedAt { get; set; }

    public string FilePath { get; set; } // PDF location
}