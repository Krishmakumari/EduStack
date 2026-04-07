namespace PaymentService.Application.DTOs.Requests;

public class ConfirmPaymentRequest
{
    public string TransactionId { get; set; } = default!;
}