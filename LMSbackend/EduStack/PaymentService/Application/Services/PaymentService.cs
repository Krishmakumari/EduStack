using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.DTOs.Responses;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IRefundRepository _refundRepo;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IRefundRepository refundRepo)
    {
        _paymentRepo = paymentRepo;
        _refundRepo = refundRepo;
    }

    // ─── Initiate Payment ─────────────────────────────────────────────────────
    public async Task<PaymentResponse> InitiatePaymentAsync(
        Guid studentId, string studentName, InitiatePaymentRequest request)
    {
        // Check if already paid for this course
        var existing = await _paymentRepo
            .GetByStudentAndCourseAsync(studentId, request.CourseId);

        if (existing is not null && existing.Status == PaymentStatus.Completed)
            throw new DomainException("You have already purchased this course.");

        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            throw new DomainException("Invalid payment method.");

        var payment = Payment.Create(
            studentId,
            studentName,
            request.CourseId,
            request.CourseTitle,
            request.Amount,
            method);

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Confirm Payment ──────────────────────────────────────────────────────
    public async Task<PaymentResponse> ConfirmPaymentAsync(
        Guid studentId, Guid paymentId, ConfirmPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        payment.MarkCompleted(request.TransactionId);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Fail Payment ─────────────────────────────────────────────────────────
    public async Task<PaymentResponse> FailPaymentAsync(
        Guid studentId, Guid paymentId, FailPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        payment.MarkFailed(request.Reason);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Refund Payment ───────────────────────────────────────────────────────
    public async Task<PaymentResponse> RefundPaymentAsync(
        Guid studentId, Guid paymentId, RefundPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        payment.MarkRefunded();

        var refund = Refund.Create(
            paymentId,
            studentId,
            payment.Amount,
            request.Reason);

        await _refundRepo.AddAsync(refund);
        await _refundRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Get My Payments ──────────────────────────────────────────────────────
    public async Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(Guid studentId)
    {
        var payments = await _paymentRepo.GetByStudentIdAsync(studentId);
        return payments.Select(MapToPaymentResponse);
    }

    // ─── Get Payment By ID ────────────────────────────────────────────────────
    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid studentId, Guid paymentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        return MapToPaymentResponse(payment);
    }

    // ─── Get Payments By Course (Instructor/Admin) ────────────────────────────
    public async Task<IEnumerable<PaymentResponse>> GetPaymentsByCourseAsync(Guid courseId)
    {
        var payments = await _paymentRepo.GetByCourseIdAsync(courseId);
        return payments.Select(MapToPaymentResponse);
    }

    // ─── Mapping Helper ───────────────────────────────────────────────────────
    private static PaymentResponse MapToPaymentResponse(Payment p) => new()
    {
        PaymentId = p.PaymentId,
        StudentId = p.StudentId,
        StudentName = p.StudentName,
        CourseId = p.CourseId,
        CourseTitle = p.CourseTitle,
        Amount = p.Amount,
        Status = p.Status.ToString(),
        Method = p.Method.ToString(),
        TransactionId = p.TransactionId,
        FailureReason = p.FailureReason,
        CreatedAt = p.CreatedAt,
        CompletedAt = p.CompletedAt,
        RefundedAt = p.RefundedAt
    };
}