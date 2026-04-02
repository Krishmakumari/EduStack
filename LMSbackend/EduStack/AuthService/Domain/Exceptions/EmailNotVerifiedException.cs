namespace AuthService.Domain.Exceptions;

public class EmailNotVerifiedException()
    : DomainException("Please verify your email before logging in.");