// DomainException — Base exception for business rule violations in Payment Service.
// • All domain-specific exceptions inherit from this.
// • Caught by ExceptionMiddleware and mapped to 400 Bad Request.

namespace PaymentService.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}