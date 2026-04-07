using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; private set; }
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; } = default!;
    public Guid CourseId { get; private set; }
    public string CourseTitle { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private Payment() { }

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
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted(string transactionId)
    {
        if (Status == PaymentStatus.Completed)
            throw new PaymentAlreadyCompletedException();

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    public void MarkRefunded()
    {
        if (Status != PaymentStatus.Completed)
            throw new DomainException("Only completed payments can be refunded.");

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }
}