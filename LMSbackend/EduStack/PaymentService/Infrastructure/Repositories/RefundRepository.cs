// RefundRepository — EF Core implementation for Refund persistence.
// • GetByPaymentIdAsync: find the refund for a specific payment (one-to-one in practice).
// • AddAsync + SaveChangesAsync: split to follow the same pattern as PaymentRepository.

using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly PaymentDbContext _context;

    public RefundRepository(PaymentDbContext context)
    {
        _context = context;
    }

    // Find the refund associated with a specific payment.
    // One payment → at most one refund (current design: full refund model only).
    public async Task<Refund?> GetByPaymentIdAsync(Guid paymentId)
        => await _context.Refunds
            .FirstOrDefaultAsync(r => r.PaymentId == paymentId);

    // Mark Refund as Added — INSERT runs on SaveChangesAsync.
    public async Task AddAsync(Refund refund)
        => await _context.Refunds.AddAsync(refund);

    // Commit the new Refund record to the database.
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}