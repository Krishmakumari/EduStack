// PaymentNotFoundException — Thrown when a paymentId doesn't exist in the database.
// • Primary constructor syntax — compact definition.
// • Mapped to 404 Not Found by ExceptionMiddleware (explicit catch block before DomainException).

namespace PaymentService.Domain.Exceptions;

public class PaymentNotFoundException()
    : DomainException("Payment not found.");