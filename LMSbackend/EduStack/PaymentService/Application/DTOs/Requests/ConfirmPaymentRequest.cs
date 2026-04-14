// ConfirmPaymentRequest — Input for marking a payment as Completed.
// • TransactionId: the reference ID returned by the payment gateway on success.
//   Stored on Payment entity as proof of payment (e.g., Razorpay ID, Stripe charge ID).

namespace PaymentService.Application.DTOs.Requests;

public class ConfirmPaymentRequest
{
    public string TransactionId { get; set; } = default!;
}