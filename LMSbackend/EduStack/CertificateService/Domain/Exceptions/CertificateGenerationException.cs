namespace CertificateService.Domain.Exceptions;

public class CertificateGenerationException : Exception
{
    public CertificateGenerationException(string message)
        : base(message)
    {
    }
}