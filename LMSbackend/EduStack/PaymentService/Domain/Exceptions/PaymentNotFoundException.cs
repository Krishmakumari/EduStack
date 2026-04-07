namespace PaymentService.Domain.Exceptions;

public class PaymentNotFoundException()
    : DomainException("Payment not found.");