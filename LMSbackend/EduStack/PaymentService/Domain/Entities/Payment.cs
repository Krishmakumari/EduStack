// Payment Entity — Tracks a student's course purchase with full lifecycle state.
// • DDD: private constructor + factory method ensures valid initial state (Pending).
// • Private setters: all state changes go through domain methods (MarkCompleted, etc.).
// • Denormalized StudentName + CourseTitle: captured at purchase time for history.
// • Guard methods enforce business rules: can't double-confirm, only Completed can refund.

using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; private set; }     // PK

    public Guid StudentId { get; private set; }      // from JWT "sub" claim
    public string StudentName { get; private set; } = default!; // denormalized from JWT

    public Guid CourseId { get; private set; }       // which course being purchased
    public string CourseTitle { get; private set; } = default!; // denormalized from request

    public decimal Amount { get; private set; }      // price at time of purchase (immutable)

    // Current status in the payment lifecycle.
    // Stored as string in DB (HasConversion<string>) for SQL readability.
    public PaymentStatus Status { get; private set; }

    // Payment method (UPI, CreditCard, etc.) — stored as string in DB.
    public PaymentMethod Method { get; private set; }

    // Set only on successful payment (MarkCompleted). Null until then.
    // This is the gateway's transaction reference — proof of payment.
    public string? TransactionId { get; private set; }

    // Set only on failed payment (MarkFailed). Null unless payment failed.
    public string? FailureReason { get; private set; }

    public DateTime CreatedAt { get; private set; }   // when initiated (UTC)
    public DateTime? CompletedAt { get; private set; } // null until Confirmed
    public DateTime? RefundedAt { get; private set; }  // null unless Refunded

    // Private constructor — forces use of Create() factory method.
    private Payment() { }

    // Create() — factory method; the ONLY way to create a valid Payment.
    // Always starts in Pending status — no payment can skip this state.
    public static Payment Create(
        Guid studentId,
        string studentName,
        Guid courseId,
        string courseTitle,
        decimal amount,
        PaymentMethod method)
    {
        return new Payment
        {
            PaymentId = Guid.NewGuid(),
            StudentId = studentId,
            StudentName = studentName,
            CourseId = courseId,
            CourseTitle = courseTitle,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,   // always starts Pending
            CreatedAt = DateTime.UtcNow
        };
    }

    // MarkCompleted — transitions Pending → Completed.
    // Guard: throws PaymentAlreadyCompletedException if called again (prevents double-confirm).
    // TransactionId: gateway reference ID, required proof of successful payment.
    public void MarkCompleted(string transactionId)
    {
        if (Status == PaymentStatus.Completed)
            throw new PaymentAlreadyCompletedException();   // domain guard — no double-confirm

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;   // store gateway's transaction reference
        CompletedAt = DateTime.UtcNow;
    }

    // MarkFailed — transitions the payment to Failed state.
    // Stores the reason (e.g., "Insufficient funds", "Card declined").
    // No guard — any state can be marked Failed (gateways can report failure at any point).
    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    // MarkRefunded — transitions Completed → Refunded.
    // Guard: only Completed payments can be refunded (can't refund Pending or Failed).
    // RefundedAt recorded for audit trail.
    public void MarkRefunded()
    {
        if (Status != PaymentStatus.Completed)
            throw new DomainException("Only completed payments can be refunded.");

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }
}