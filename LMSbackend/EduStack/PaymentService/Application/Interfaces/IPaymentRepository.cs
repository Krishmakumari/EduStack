// IPaymentRepository — Repository contract for Payment persistence.
// • GetByStudentAndCourseAsync: used for duplicate purchase check — returns null if not found.
// • Separate AddAsync and SaveChangesAsync — explicit commit control in the service.

using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    // Find one payment by its PK.
    Task<Payment?> GetByIdAsync(Guid paymentId);

    // All payments for a student (purchase history) — ordered most recent first.
    Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId);

    // All payments for a course — Admin/Instructor revenue view.
    Task<IEnumerable<Payment>> GetByCourseIdAsync(Guid courseId);

    // Check if student already has a payment for this course (for duplicate prevention).
    // Returns null = no existing payment. Returns record = check its Status.
    Task<Payment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);

    // Get all payments across the system.
    Task<IEnumerable<Payment>> GetAllAsync();

    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}