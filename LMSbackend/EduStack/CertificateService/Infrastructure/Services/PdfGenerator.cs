// PdfGenerator — Creates certificate PDFs in memory using QuestPDF (Infrastructure Layer).
// • QuestPDF is a free, open-source .NET library with a fluent code-first API.
// • No HTML-to-PDF conversion, no external browser (Chrome/Puppeteer) needed.
// • Returns byte[] — the raw PDF content. CertificateService saves it to disk.

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CertificateService.Infrastructure.Services;

public class PdfGenerator
{
    // GenerateCertificate — builds the PDF layout and returns raw bytes.
    // Layout: centered title → "This certifies that" → student name → course → date.
    // All content is centered and uses varying font sizes for visual hierarchy.
    public byte[] GenerateCertificate(string userName, string courseTitle)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);    // 50pt margin on all sides

                page.Content()
                    .Column(col =>
                    {
                        // Title — large, bold heading
                        col.Item().AlignCenter().Text("Certificate of Completion")
                            .FontSize(30).Bold();

                        // "This certifies that" — transition text
                        col.Item().PaddingTop(20)
                            .AlignCenter()
                            .Text($"This certifies that");

                        // Student name — prominent, bold
                        col.Item().AlignCenter()
                            .Text(userName)
                            .FontSize(24).Bold();

                        // "has successfully completed the course" — transition text
                        col.Item().AlignCenter()
                            .Text($"has successfully completed the course");

                        // Course title — slightly smaller than student name
                        col.Item().AlignCenter()
                            .Text(courseTitle)
                            .FontSize(20).Bold();

                        // Issue date — formatted as "14 Apr 2026"
                        col.Item().PaddingTop(20)
                            .AlignCenter()
                            .Text($"Date: {DateTime.UtcNow:dd MMM yyyy}");
                    });
            });
        }).GeneratePdf();  // GeneratePdf() returns byte[] — entire PDF in memory
    }
}