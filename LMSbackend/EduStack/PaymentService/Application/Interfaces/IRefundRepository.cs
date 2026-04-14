// IRefundRepository — Repository contract for Refund persistence.
// • GetByPaymentIdAsync: find refund by its parent payment (used for audit/status checks).

using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IRefundRepository
{
    // Find the refund record associated with a specific payment.
    // Returns null if no refund has been created for this payment yet.
    Task<Refund?> GetByPaymentIdAsync(Guid paymentId);

    Task AddAsync(Refund refund);
    Task SaveChangesAsync();
}