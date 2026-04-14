// UnauthorizedPaymentAccessException — Thrown when a student accesses another student's payment.
// • Triggered by the ownership check: payment.StudentId != studentId (from JWT).
// • Mapped to 403 Forbidden — student IS authenticated, just not the payment's owner.

namespace PaymentService.Domain.Exceptions;

public class UnauthorizedPaymentAccessException()
    : DomainException("You are not authorized to access this payment.");