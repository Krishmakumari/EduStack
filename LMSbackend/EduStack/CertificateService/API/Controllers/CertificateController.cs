// CertificateController — HTTP entry point for certificate generation and download.
// • 2 endpoints: POST /generate (create PDF) and GET /download/{id} (stream PDF).
// • No [Authorize] yet — in production, add JWT auth to restrict access.
// • Controller is thin — delegates all logic to ICertificateService.

using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CertificateService.API.Controllers;

[ApiController]
[Route("api/certificates")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificateController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    // POST api/certificate/generate
    // Creates a PDF certificate, saves to disk + DB, returns the certificate ID.
    // Uses POST because it creates a resource (has side effects: file + DB write).
    // Input: GenerateCertificateDto with UserId, CourseId, UserName, CourseTitle.
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateCertificateDto dto)
    {
        var result = await _certificateService.GenerateCertificateAsync(dto);
        return Ok(result);
    }

    // GET api/certificate/download/{id}
    // Streams the PDF file to the browser as a downloadable file.
    // File() sets Content-Type: application/pdf and Content-Disposition: attachment.
    // The browser will prompt the user to save "certificate.pdf".
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var fileBytes = await _certificateService.DownloadCertificateAsync(id);

        // File() is a built-in ControllerBase method that returns FileContentResult.
        // "application/pdf" = MIME type, "certificate.pdf" = suggested download filename.
        return File(fileBytes, "application/pdf", "certificate.pdf");
    }
}