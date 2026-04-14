// PaymentResponse — Full payment response DTO returned by all payment operations.
// • Status and Method are strings (not enums) for JSON readability.
// • Three nullable timestamps reflect the lifecycle: null = "hasn't happened yet".
//   CreatedAt: always set. CompletedAt: set on Confirm. RefundedAt: set on Refund.

namespace PaymentService.Application.DTOs.Responses;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;   // denormalized from JWT at initiation
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;   // denormalized from request at initiation

    public decimal Amount { get; set; }                   // price locked at initiation

    public string Status { get; set; } = default!;        // "Pending"/"Completed"/"Failed"/"Refunded"
    public string Method { get; set; } = default!;        // "UPI"/"CreditCard" etc.

    public string? TransactionId { get; set; }            // null until Completed
    public string? FailureReason { get; set; }            // null unless Failed

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }            // null until Confirmed
    public DateTime? RefundedAt { get; set; }             // null unless Refunded
}