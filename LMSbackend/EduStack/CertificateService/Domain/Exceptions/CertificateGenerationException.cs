// CertificateGenerationException — Thrown when certificate creation or file access fails.
// • Triggered when the DB record exists but the PDF file is missing from disk.
// • Defense-in-depth: DB and filesystem can get out of sync (manual deletion, deployment).

namespace CertificateService.Domain.Exceptions;

public class CertificateGenerationException : Exception
{
    public CertificateGenerationException(string message)
        : base(message)
    {
    }
}