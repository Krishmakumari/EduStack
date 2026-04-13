// CertificateNotFoundException — Thrown when a certificate ID doesn't exist in the database.
// • Triggered during download when the GUID doesn't match any record.
// • No GlobalExceptionMiddleware yet — returns as 500 by default (add middleware in production).

namespace CertificateService.Domain.Exceptions;

public class CertificateNotFoundException : Exception
{
    public CertificateNotFoundException(Guid id)
        : base($"Certificate with ID {id} not found.")
    {
    }
}