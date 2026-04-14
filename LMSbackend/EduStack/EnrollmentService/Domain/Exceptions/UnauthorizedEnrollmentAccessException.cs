// UnauthorizedEnrollmentAccessException — Thrown when a student accesses another student's enrollment.
// • Triggered by the ownership check: enrollment.StudentId != studentId (from JWT).
// • Mapped to 403 Forbidden in middleware — authenticated but not authorized.

namespace EnrollmentService.Domain.Exceptions;

public class UnauthorizedEnrollmentAccessException()
    : DomainException("You are not authorized to access this enrollment.");