// GenerateCertificateDto — Input for certificate generation.
// • Contains both IDs (for DB storage) and names (for PDF rendering).
// • UserName/CourseTitle are passed by the client for now.
//   Future: fetch from Auth/Course services using the IDs.

namespace CertificateService.Application.DTOs;

public class GenerateCertificateDto
{
    public Guid UserId { get; set; }        // who is requesting the certificate
    public Guid CourseId { get; set; }      // for which course

    public string UserName { get; set; }    // display name (printed on the PDF)
    public string CourseTitle { get; set; }  // course name (printed on the PDF)
}