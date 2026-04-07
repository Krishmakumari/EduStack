namespace PaymentService.Application.DTOs.Responses;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public string Method { get; set; } = default!;
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}