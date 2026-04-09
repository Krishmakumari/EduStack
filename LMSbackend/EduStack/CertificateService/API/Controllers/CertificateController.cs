using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CertificateService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificateController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    // ✅ Generate Certificate
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateCertificateDto dto)
    {
        var result = await _certificateService.GenerateCertificateAsync(dto);
        return Ok(result);
    }

    // ✅ Download Certificate
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var fileBytes = await _certificateService.DownloadCertificateAsync(id);

        return File(fileBytes, "application/pdf", "certificate.pdf");
    }
}