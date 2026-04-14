// Refund Entity — Tracks a refund record when a payment is reversed.
// • Created atomically with payment.MarkRefunded() in PaymentService.RefundPaymentAsync.
// • Separate entity from Payment — both records preserved for full audit trail.
// • Amount copied from Payment.Amount (full refund model — no partial refunds currently).
// • TransactionId: gateway's refund reference ID (set externally, not at creation).

namespace PaymentService.Domain.Entities;

public class Refund
{
    public Guid RefundId { get; private set; }    // PK

    public Guid PaymentId { get; private set; }   // FK → Payment (cascade delete)
    public Guid StudentId { get; private set; }   // denormalized — quick access without join

    public decimal Amount { get; private set; }   // full refund amount (copied from Payment)

    // Why the refund was requested — stored for audit trail and dispute resolution.
    public string Reason { get; private set; } = default!;

    // Gateway's refund transaction ID. null until the refund is processed externally.
    public string? TransactionId { get; private set; }

    public DateTime CreatedAt { get; private set; }  // when the refund was initiated (UTC)

    // Navigation property — EF Core link back to parent Payment.
    public Payment Payment { get; private set; } = default!;

    // Private constructor — forces use of Create() factory method.
    private Refund() { }

    // Create() — creates a Refund record at the time of refund initiation.
    // TransactionId not set here — it comes from the payment gateway after processing.
    public static Refund Create(
        Guid paymentId,
        Guid studentId,
        decimal amount,
        string reason)
    {
        return new Refund
        {
            RefundId = Guid.NewGuid(),
            PaymentId = paymentId,
            StudentId = studentId,
            Amount = amount,     // full refund of original payment amount
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }

    // SetTransactionId — called when the payment gateway processes the refund.
    // This is the external confirmation reference for the refund.
    public void SetTransactionId(string transactionId)
        => TransactionId = transactionId;
}