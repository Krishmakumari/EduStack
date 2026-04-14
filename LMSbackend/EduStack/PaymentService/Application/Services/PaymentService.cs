// PaymentService — Core business logic for the 4-state payment lifecycle.
// • Lifecycle: Pending → Completed | Failed → Refunded (via domain methods on entity).
// • Two repositories: IPaymentRepository for payments, IRefundRepository for refunds.
// • Ownership validation on every write: studentId from JWT must match Payment.StudentId.
// • Duplicate check before InitiatePayment: blocks if a Completed payment exists.
// • String → Enum parsing for PaymentMethod: tolerant of case differences.

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
    // Creates a new payment record in Pending status.
    // DUPLICATE CHECK: blocks if student already has a Completed payment for this course.
    // Failed payments are NOT blocked — student can retry after a failed attempt.
    // StudentId and StudentName come from JWT; CourseTitle denormalized from request.
    public async Task<PaymentResponse> InitiatePaymentAsync(
        Guid studentId, string studentName, InitiatePaymentRequest request)
    {
        // Step 1: Check if student already successfully paid for this course.
        var existing = await _paymentRepo
            .GetByStudentAndCourseAsync(studentId, request.CourseId);

        if (existing is not null && existing.Status == PaymentStatus.Completed)
            throw new DomainException("You have already purchased this course.");

        // Step 2: Parse payment method string → enum (case-insensitive).
        // "upi", "UPI", "Upi" → all valid. Invalid string → 400.
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            throw new DomainException("Invalid payment method.");

        // Step 3: Create payment via DDD factory — sets Status=Pending, CreatedAt=UtcNow.
        var payment = Payment.Create(
            studentId,
            studentName,      // denormalized from JWT "name" claim
            request.CourseId,
            request.CourseTitle,  // denormalized from request — no Course Service call
            request.Amount,
            method);

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Confirm Payment ──────────────────────────────────────────────────────
    // Called after the payment gateway confirms the transaction.
    // Transitions status: Pending → Completed.
    // TransactionId: gateway-issued proof of payment (e.g., Razorpay/Stripe tx ID).
    // Guard on entity: throws PaymentAlreadyCompletedException if already Completed.
    public async Task<PaymentResponse> ConfirmPaymentAsync(
        Guid studentId, Guid paymentId, ConfirmPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        // Ownership check — only the student who initiated can confirm their own payment.
        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        // Domain method enforces the transition and sets TransactionId + CompletedAt.
        payment.MarkCompleted(request.TransactionId);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Fail Payment ─────────────────────────────────────────────────────────
    // Called when the payment gateway signals failure (e.g., declined card).
    // Transitions status: Pending → Failed. Stores the failure reason.
    // No guard: any status can be marked Failed (real-world gateways can fail late).
    public async Task<PaymentResponse> FailPaymentAsync(
        Guid studentId, Guid paymentId, FailPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        // Domain method sets Status=Failed + FailureReason.
        payment.MarkFailed(request.Reason);
        await _paymentRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);
    }

    // ─── Refund Payment ───────────────────────────────────────────────────────
    // Refunds a completed payment. Transitions: Completed → Refunded.
    // Guard on entity: only Completed payments can be refunded (no refund on failed/pending).
    // Creates a separate Refund entity — preserves both the original payment + refund records.
    public async Task<PaymentResponse> RefundPaymentAsync(
        Guid studentId, Guid paymentId, RefundPaymentRequest request)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        // Entity guard: throws DomainException if Status != Completed.
        payment.MarkRefunded();  // sets Status=Refunded, RefundedAt=UtcNow

        // Create Refund record — separate entity tracks the refund details.
        // Amount copied from original payment (full refund model).
        var refund = Refund.Create(
            paymentId,
            studentId,
            payment.Amount,  // full refund of the original amount
            request.Reason);

        await _refundRepo.AddAsync(refund);
        await _refundRepo.SaveChangesAsync();

        return MapToPaymentResponse(payment);  // returns updated payment (now Refunded)
    }

    // ─── Get My Payments ──────────────────────────────────────────────────────
    // Returns all payments for the JWT student (purchase history).
    public async Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(Guid studentId)
    {
        var payments = await _paymentRepo.GetByStudentIdAsync(studentId);
        return payments.Select(MapToPaymentResponse);
    }

    // ─── Get Payment By ID ────────────────────────────────────────────────────
    // Returns one payment with ownership validation — students see only their own.
    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid studentId, Guid paymentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new PaymentNotFoundException();

        if (payment.StudentId != studentId)
            throw new UnauthorizedPaymentAccessException();

        return MapToPaymentResponse(payment);
    }

    // ─── Get Payments By Course (Admin/Instructor) ────────────────────────────
    // Returns all student payments for a course — for revenue dashboards.
    // No ownership check — Admin/Instructor can see all students' payments.
    public async Task<IEnumerable<PaymentResponse>> GetPaymentsByCourseAsync(Guid courseId)
    {
        var payments = await _paymentRepo.GetByCourseIdAsync(courseId);
        return payments.Select(MapToPaymentResponse);
    }

    // ─── Mapping Helper ───────────────────────────────────────────────────────
    // Manual mapping instead of AutoMapper — simpler and easier to debug.
    // Status and Method enums converted to strings for JSON readability.
    // Nullable timestamps (CompletedAt, RefundedAt) remain null in JSON if not set.
    private static PaymentResponse MapToPaymentResponse(Payment p) => new()
    {
        PaymentId = p.PaymentId,
        StudentId = p.StudentId,
        StudentName = p.StudentName,
        CourseId = p.CourseId,
        CourseTitle = p.CourseTitle,
        Amount = p.Amount,
        Status = p.Status.ToString(),       // enum → string: "Completed" not 1
        Method = p.Method.ToString(),       // enum → string: "UPI" not 2
        TransactionId = p.TransactionId,    // null until Confirmed
        FailureReason = p.FailureReason,    // null unless Failed
        CreatedAt = p.CreatedAt,
        CompletedAt = p.CompletedAt,        // null until Confirmed
        RefundedAt = p.RefundedAt           // null until Refunded
    };
}