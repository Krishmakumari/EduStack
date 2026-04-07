using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.DTOs.Responses;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> InitiatePaymentAsync(Guid studentId, string studentName, InitiatePaymentRequest request);
    Task<PaymentResponse> ConfirmPaymentAsync(Guid studentId, Guid paymentId, ConfirmPaymentRequest request);
    Task<PaymentResponse> FailPaymentAsync(Guid studentId, Guid paymentId, FailPaymentRequest request);
    Task<PaymentResponse> RefundPaymentAsync(Guid studentId, Guid paymentId, RefundPaymentRequest request);
    Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(Guid studentId);
    Task<PaymentResponse> GetPaymentByIdAsync(Guid studentId, Guid paymentId);
    Task<IEnumerable<PaymentResponse>> GetPaymentsByCourseAsync(Guid courseId);
}