namespace AuthService.Domain.Exceptions;

public class InvalidCredentialsException()
    : DomainException("Invalid email or password.");