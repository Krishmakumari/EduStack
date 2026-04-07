namespace EnrollmentService.Domain.Exceptions;

public class AlreadyEnrolledException()
    : DomainException("You are already enrolled in this course.");