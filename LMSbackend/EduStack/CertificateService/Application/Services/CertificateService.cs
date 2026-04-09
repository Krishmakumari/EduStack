using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using CertificateService.Domain.Entities;
using CertificateService.Domain.Exceptions;
using CertificateService.Infrastructure.Persistence;
using CertificateService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CertificateService.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly CertificateDbContext _context;
    private readonly PdfGenerator _pdfGenerator;

    public CertificateService(
        CertificateDbContext context,
        PdfGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<CertificateResponseDto> GenerateCertificateAsync(GenerateCertificateDto dto)
    {
        // 🔥 (Later: check QuizService if passed)

        // Generate PDF
        var pdfBytes = _pdfGenerator.GenerateCertificate(dto.UserName, dto.CourseTitle);

        // Save file locally
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllBytesAsync(filePath, pdfBytes);

        // Save in DB
        var certificate = new Certificate
        {
            CertificateId = Guid.NewGuid(),
            UserId = dto.UserId,
            CourseId = dto.CourseId,
            UserName = dto.UserName,
            CourseTitle = dto.CourseTitle,
            IssuedAt = DateTime.UtcNow,
            FilePath = filePath
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();

        return new CertificateResponseDto
        {
            CertificateId = certificate.CertificateId,
            FilePath = filePath
        };
    }

    public async Task<byte[]> DownloadCertificateAsync(Guid certificateId)
    {
        var cert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

        if (cert == null)
            throw new CertificateNotFoundException(certificateId);

        if (!File.Exists(cert.FilePath))
            throw new CertificateGenerationException("Certificate file not found.");

        return await File.ReadAllBytesAsync(cert.FilePath);
    }
}