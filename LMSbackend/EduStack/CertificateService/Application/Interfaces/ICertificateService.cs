// ICertificateService — Contract for certificate operations.
// • Dependency Inversion: controller depends on this interface, not the concrete class.
// • Two operations: generate (create PDF + save) and download (read PDF bytes).

using CertificateService.Application.DTOs;

namespace CertificateService.Application.Interfaces;

public interface ICertificateService
{
    // Generates a PDF certificate, saves to disk + DB, returns the cert ID.
    Task<CertificateResponseDto> GenerateCertificateAsync(GenerateCertificateDto dto);

    // Reads the PDF file from disk and returns raw bytes for HTTP file download.
    Task<byte[]> DownloadCertificateAsync(Guid certificateId);
}