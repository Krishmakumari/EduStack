using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CertificateService.Infrastructure.Services;

public class PdfGenerator
{
    public byte[] GenerateCertificate(string userName, string courseTitle)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);

                page.Content()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("Certificate of Completion")
                            .FontSize(30).Bold();

                        col.Item().PaddingTop(20)
                            .AlignCenter()
                            .Text($"This certifies that");

                        col.Item().AlignCenter()
                            .Text(userName)
                            .FontSize(24).Bold();

                        col.Item().AlignCenter()
                            .Text($"has successfully completed the course");

                        col.Item().AlignCenter()
                            .Text(courseTitle)
                            .FontSize(20).Bold();

                        col.Item().PaddingTop(20)
                            .AlignCenter()
                            .Text($"Date: {DateTime.UtcNow:dd MMM yyyy}");
                    });
            });
        }).GeneratePdf();
    }
}