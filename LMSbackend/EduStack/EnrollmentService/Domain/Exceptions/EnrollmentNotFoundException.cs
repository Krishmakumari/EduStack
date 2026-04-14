// EnrollmentNotFoundException — Thrown when an enrollment ID doesn't exist in the database.
// • Uses C# primary constructor syntax — compact exception definition.
// • Mapped to 404 Not Found in GlobalExceptionMiddleware.

namespace EnrollmentService.Domain.Exceptions;

public class EnrollmentNotFoundException()
    : DomainException("Enrollment not found.");