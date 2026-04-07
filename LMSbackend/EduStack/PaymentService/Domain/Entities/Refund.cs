namespace PaymentService.Domain.Entities;

public class Refund
{
    public Guid RefundId { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid StudentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = default!;
    public string? TransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Payment Payment { get; private set; } = default!;

    private Refund() { }

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
            Amount = amount,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetTransactionId(string transactionId)
        => TransactionId = transactionId;
}