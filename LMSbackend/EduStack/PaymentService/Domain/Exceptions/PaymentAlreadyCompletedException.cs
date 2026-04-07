namespace PaymentService.Domain.Exceptions;

public class PaymentAlreadyCompletedException()
    : DomainException("Payment has already been completed.");