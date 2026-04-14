// DomainException — Base exception for all business rule violations in Enrollment Service.
// • Caught by GlobalExceptionMiddleware and mapped to 400 Bad Request.
// • All domain-specific exceptions (AlreadyEnrolled, NotFound, etc.) inherit from this.

namespace EnrollmentService.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}