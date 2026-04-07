namespace PaymentService.Application.DTOs.Requests;

public class InitiatePaymentRequest
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "UPI";
}