namespace PaymentService.Domain.Exceptions;

public class UnauthorizedPaymentAccessException()
    : DomainException("You are not authorized to access this payment.");