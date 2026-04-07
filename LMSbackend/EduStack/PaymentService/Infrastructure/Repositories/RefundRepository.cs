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

    public async Task<Refund?> GetByPaymentIdAsync(Guid paymentId)
        => await _context.Refunds
            .FirstOrDefaultAsync(r => r.PaymentId == paymentId);

    public async Task AddAsync(Refund refund)
        => await _context.Refunds.AddAsync(refund);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}