// CertificateService — Core business logic for certificate generation (Application Layer).
// • Generates PDF using QuestPDF, saves to local disk, records metadata in DB.
// • Two operations: GenerateCertificate (create) and DownloadCertificate (read).
// • No repository pattern — DbContext injected directly (single entity, simple ops).

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
    // DbContext injected directly instead of through a repository.
    // Acceptable for simple services with one entity and two operations.
    private readonly CertificateDbContext _context;

    // PdfGenerator handles the actual QuestPDF document creation.
    private readonly PdfGenerator _pdfGenerator;

    public CertificateService(
        CertificateDbContext context,
        PdfGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    // ─── Generate Certificate ─────────────────────────────────────────────────
    // Creates a PDF certificate, saves it to disk, and records the metadata in DB.
    // Flow: validate → generate PDF bytes → save file → save DB record → return ID.
    public async Task<CertificateResponseDto> GenerateCertificateAsync(GenerateCertificateDto dto)
    {
        // TODO: Future enhancement — verify user passed the quiz before issuing.
        // Would use QuizClient.HasUserPassed(dto.UserId, dto.CourseId).
        // 🔥 (Later: check QuizService if passed)

        // Step 1: Generate the PDF in memory using QuestPDF.
        // Returns raw byte[] — no file on disk yet.
        var pdfBytes = _pdfGenerator.GenerateCertificate(dto.UserName, dto.CourseTitle);

        // Step 2: Save PDF to local filesystem.
        // In production, replace with Azure Blob Storage or AWS S3.
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Each certificate gets a unique GUID filename to avoid collisions.
        var fileName = $"{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllBytesAsync(filePath, pdfBytes);

        // Step 3: Save certificate metadata in the database.
        // UserName and CourseTitle are denormalized — captured at generation time
        // so the certificate preserves the original names even if they change later.
        var certificate = new Certificate
        {
            CertificateId = Guid.NewGuid(),
            UserId = dto.UserId,         // who earned it (from Auth Service)
            CourseId = dto.CourseId,      // which course (from Course Service)
            UserName = dto.UserName,     // denormalized for display
            CourseTitle = dto.CourseTitle, // denormalized for display
            IssuedAt = DateTime.UtcNow,
            FilePath = filePath          // local path to the PDF file
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();

        // Step 4: Return the certificate ID (client uses this to download later).
        return new CertificateResponseDto
        {
            CertificateId = certificate.CertificateId,
            FilePath = filePath
        };
    }

    // ─── Download Certificate ─────────────────────────────────────────────────
    // Reads the PDF file from disk and returns raw bytes for HTTP file download.
    // Two failure modes: DB record missing OR physical file missing on disk.
    public async Task<byte[]> DownloadCertificateAsync(Guid certificateId)
    {
        // Step 1: Look up the certificate record by ID.
        var cert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

        // Step 2: If no record in DB → 404-style error.
        if (cert == null)
            throw new CertificateNotFoundException(certificateId);

        // Step 3: If record exists but file was deleted from disk → runtime error.
        // This is defense-in-depth: DB and filesystem can get out of sync.
        if (!File.Exists(cert.FilePath))
            throw new CertificateGenerationException("Certificate file not found.");

        // Step 4: Read and return the raw PDF bytes.
        // Controller wraps this in File() result → Content-Type: application/pdf.
        return await File.ReadAllBytesAsync(cert.FilePath);
    }
}