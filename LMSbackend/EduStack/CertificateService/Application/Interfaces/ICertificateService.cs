using CertificateService.Application.DTOs;

namespace CertificateService.Application.Interfaces;

public interface ICertificateService
{
    Task<CertificateResponseDto> GenerateCertificateAsync(GenerateCertificateDto dto);
    Task<byte[]> DownloadCertificateAsync(Guid certificateId);
}