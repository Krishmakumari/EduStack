// PaymentAlreadyCompletedException — Thrown when MarkCompleted() is called on a Completed payment.
// • Primary constructor syntax — compact definition.
// • Triggered by Payment.MarkCompleted() domain guard.
// • Mapped to 400 Bad Request (inherits DomainException) via ExceptionMiddleware.

namespace PaymentService.Domain.Exceptions;

public class PaymentAlreadyCompletedException()
    : DomainException("Payment has already been completed.");