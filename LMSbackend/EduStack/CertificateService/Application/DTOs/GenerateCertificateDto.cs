namespace CertificateService.Application.DTOs;

public class GenerateCertificateDto
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    public string UserName { get; set; }   // for now (later from AuthService)
    public string CourseTitle { get; set; } // later from CourseService
}