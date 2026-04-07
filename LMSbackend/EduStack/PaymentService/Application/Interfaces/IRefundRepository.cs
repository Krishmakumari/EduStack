using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IRefundRepository
{
    Task<Refund?> GetByPaymentIdAsync(Guid paymentId);
    Task AddAsync(Refund refund);
    Task SaveChangesAsync();
}