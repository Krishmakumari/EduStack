// PaymentStatus Enum — Lifecycle states for a payment transaction.
// • Stored as STRING in DB (HasConversion<string>()) for SQL readability.
// • Valid transitions: Pending → Completed | Failed. Completed → Refunded.
// • Refunded can only be reached from Completed (enforced by MarkRefunded() guard).

namespace PaymentService.Domain.Enums;

public enum PaymentStatus
{
    Pending,    // default — payment initiated, awaiting gateway confirmation
    Completed,  // gateway confirmed successful payment (TransactionId set)
    Failed,     // gateway reported failure (FailureReason set)
    Refunded    // completed payment reversed (Refund entity created, RefundedAt set)
}