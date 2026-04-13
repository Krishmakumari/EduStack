// DomainException — Base exception for all business rule violations.
// • Caught by GlobalExceptionMiddleware and mapped to 400 Bad Request.

namespace CourseService.Domain.Exceptions
{
    public class DomainException:Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
