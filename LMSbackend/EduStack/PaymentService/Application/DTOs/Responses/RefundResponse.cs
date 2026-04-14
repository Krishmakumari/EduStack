// RefundResponse — DTO for a refund record (defined but not yet returned from any endpoint).
// • Currently the RefundPaymentAsync endpoint returns PaymentResponse (the updated payment).
// • This DTO would be used if a dedicated GET /refunds/{id} endpoint were added.

namespace PaymentService.Application.DTOs.Responses;

public class RefundResponse
{
    public Guid RefundId { get; set; }
    public Guid PaymentId { get; set; }           // which payment was refunded
    public decimal Amount { get; set; }           // refund amount (full payment amount)
    public string Reason { get; set; } = default!; // why the refund was requested
    public string? TransactionId { get; set; }    // gateway refund reference — null until processed
    public DateTime CreatedAt { get; set; }       // when refund was initiated (UTC)
}