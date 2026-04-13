// CertificateResponseDto — Output after certificate generation.
// • CertificateId: client uses this GUID to call GET /download/{id} later.
// • FilePath: where the PDF was saved on disk (informational).

namespace CertificateService.Application.DTOs;

public class CertificateResponseDto
{
    public Guid CertificateId { get; set; }  // use this to download the certificate
    public string FilePath { get; set; }     // local path where PDF was saved
}