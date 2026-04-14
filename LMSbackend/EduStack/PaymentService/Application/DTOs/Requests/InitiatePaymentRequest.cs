// InitiatePaymentRequest — Input DTO for starting a new payment.
// • CourseTitle denormalized from the request to avoid Course Service call.
// • Method: string (e.g., "UPI") — parsed to PaymentMethod enum in the service layer.
//   Default "UPI" reflects common payment method in India.

namespace PaymentService.Application.DTOs.Requests;

public class InitiatePaymentRequest
{
    public Guid CourseId { get; set; }              // which course is being purchased
    public string CourseTitle { get; set; } = default!; // denormalized — stored on Payment for history

    public decimal Amount { get; set; }             // course price at time of purchase

    // Payment method as a string — case-insensitive parsed to PaymentMethod enum.
    // Accepted: "UPI", "CreditCard", "DebitCard", "NetBanking", "Wallet"
    public string Method { get; set; } = "UPI";
}