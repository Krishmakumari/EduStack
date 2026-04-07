namespace EnrollmentService.Domain.Exceptions;

public class UnauthorizedEnrollmentAccessException()
    : DomainException("You are not authorized to access this enrollment.");