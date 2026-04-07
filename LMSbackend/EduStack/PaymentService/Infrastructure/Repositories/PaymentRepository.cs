using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid paymentId)
        => await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId)
        => await _context.Payments
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetByCourseIdAsync(Guid courseId)
        => await _context.Payments
            .Where(p => p.CourseId == courseId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Payment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        => await _context.Payments
            .FirstOrDefaultAsync(p => p.StudentId == studentId
                                   && p.CourseId == courseId);

    public async Task AddAsync(Payment payment)
        => await _context.Payments.AddAsync(payment);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}