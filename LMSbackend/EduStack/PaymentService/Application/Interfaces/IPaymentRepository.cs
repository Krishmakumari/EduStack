using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId);
    Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Payment>> GetByCourseIdAsync(Guid courseId);
    Task<Payment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}