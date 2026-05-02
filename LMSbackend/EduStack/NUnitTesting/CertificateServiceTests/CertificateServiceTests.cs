// CertificateServiceTests — Unit tests for CertificateService.
// Uses InMemory DbContext. PdfGenerator produces real PDF bytes since it's not virtual.

using Microsoft.EntityFrameworkCore;
using Moq;
using CertificateService.Application.DTOs;
using CertificateService.Domain.Exceptions;
using CertificateService.Infrastructure.Persistence;
using CertificateService.Infrastructure.Services;
using CertificateService.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace NUnitTesting.CertificateServiceTests;

[TestFixture]
public class CertificateServiceTests
{
    private CertificateDbContext _context;
    private PdfGenerator _pdfGenerator;
    private Mock<RabbitMqPublisher> _publisherMock;
    private CertificateService.Application.Services.CertificateService _certService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // QuestPDF requires a license setting — normally set in Program.cs
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CertificateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new CertificateDbContext(options);

        _pdfGenerator = new PdfGenerator(); // real instance — generates actual PDF bytes

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        _publisherMock = new Mock<RabbitMqPublisher>(configMock.Object);

        _certService = new CertificateService.Application.Services.CertificateService(
            _context, _pdfGenerator, _publisherMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GenerateCertificateAsync_ValidInput_SavesCertToDb()
    {
        // Arrange — publisher will throw (non-virtual), but DB save happens before publish
        var dto = new GenerateCertificateDto
        {
            UserId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            UserName = "John Doe",
            CourseTitle = "C# Mastery",
            UserEmail = "john@test.com"
        };

        // Act — catch the RabbitMQ connection failure
        try
        {
            await _certService.GenerateCertificateAsync(dto);
        }
        catch { /* Expected: RabbitMQ connection failure */ }

        // Assert — verify the certificate was saved to DB (core logic)
        var saved = await _context.Certificates.FirstOrDefaultAsync();
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.UserName, Is.EqualTo("John Doe"));
        Assert.That(saved.CourseTitle, Is.EqualTo("C# Mastery"));

        // Cleanup generated file
        if (File.Exists(saved.FilePath))
            File.Delete(saved.FilePath);
    }

    [Test]
    public void DownloadCertificateAsync_NotFound_ThrowsCertificateNotFound()
    {
        Assert.ThrowsAsync<CertificateNotFoundException>(
            async () => await _certService.DownloadCertificateAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task DownloadCertificateAsync_FileDeleted_ThrowsGenerationException()
    {
        // Arrange — certificate record exists but file doesn't
        var cert = new CertificateService.Domain.Entities.Certificate
        {
            CertificateId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            UserName = "Test",
            CourseTitle = "Course",
            IssuedAt = DateTime.UtcNow,
            FilePath = @"C:\nonexistent\path\file.pdf"
        };
        _context.Certificates.Add(cert);
        await _context.SaveChangesAsync();

        // Act & Assert
        Assert.ThrowsAsync<CertificateGenerationException>(
            async () => await _certService.DownloadCertificateAsync(cert.CertificateId));
    }
}
