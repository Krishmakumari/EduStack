namespace CertificateService.Domain.Exceptions;

public class CertificateNotFoundException : Exception
{
    public CertificateNotFoundException(Guid id)
        : base($"Certificate with ID {id} not found.")
    {
    }
}