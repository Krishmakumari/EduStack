// RefundPaymentRequest — Input for requesting a refund on a completed payment.
// • Reason: why the student is requesting a refund — stored on the Refund entity.
//   Required for audit trail, dispute resolution, and customer support records.

namespace PaymentService.Application.DTOs.Requests;

public class RefundPaymentRequest
{
    public string Reason { get; set; } = default!;
}