namespace PaymentService.Application.DTOs.Requests;

public class FailPaymentRequest
{
    public string Reason { get; set; } = default!;
}