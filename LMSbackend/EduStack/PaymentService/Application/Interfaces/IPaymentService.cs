// IPaymentService — Contract for all payment lifecycle operations.
// • studentId always passed separately (from JWT) — never inferred from request body.
// • Every write operation includes studentId for ownership validation in the service.

using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.DTOs.Responses;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    // Start a new payment in Pending status (duplicate-checked before creation).
    Task<PaymentResponse> InitiatePaymentAsync(Guid studentId, string studentName, InitiatePaymentRequest request);

    // Mark a Pending payment as Completed (stores gateway transaction ID).
    Task<PaymentResponse> ConfirmPaymentAsync(Guid studentId, Guid paymentId, ConfirmPaymentRequest request);

    // Mark a payment as Failed (stores the failure reason from gateway).
    Task<PaymentResponse> FailPaymentAsync(Guid studentId, Guid paymentId, FailPaymentRequest request);

    // Refund a Completed payment (creates a Refund entity, moves to Refunded status).
    Task<PaymentResponse> RefundPaymentAsync(Guid studentId, Guid paymentId, RefundPaymentRequest request);

    // Get all payments for the JWT student (purchase history).
    Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(Guid studentId);

    // Get one payment by ID with ownership check.
    Task<PaymentResponse> GetPaymentByIdAsync(Guid studentId, Guid paymentId);

    // Get all payments for a course (Admin/Instructor dashboard — no ownership check).
    Task<IEnumerable<PaymentResponse>> GetPaymentsByCourseAsync(Guid courseId);

    // Get all payments across the system (Admin only).
    Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync();
}