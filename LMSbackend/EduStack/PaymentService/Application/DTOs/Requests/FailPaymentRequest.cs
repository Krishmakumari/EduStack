// FailPaymentRequest — Input for marking a payment as Failed.
// • Reason: explanation from the payment gateway (e.g., "Insufficient funds", "Card declined").
//   Stored on Payment.FailureReason for audit trail and customer support.

namespace PaymentService.Application.DTOs.Requests;

public class FailPaymentRequest
{
    public string Reason { get; set; } = default!;
}