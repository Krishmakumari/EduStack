namespace PaymentService.Application.DTOs.Responses;

public class RefundResponse
{
    public Guid RefundId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = default!;
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}