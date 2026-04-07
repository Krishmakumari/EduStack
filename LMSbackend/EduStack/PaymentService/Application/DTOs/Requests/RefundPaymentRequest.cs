namespace PaymentService.Application.DTOs.Requests;

public class RefundPaymentRequest
{
    public string Reason { get; set; } = default!;
}