// PaymentRepository — EF Core implementation for Payment persistence.
// • GetByStudentAndCourseAsync: used for duplicate purchase check before InitiatePayment.
// • All list queries ordered by CreatedAt descending — most recent first.

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

    // Simple lookup by PK — used by Confirm, Fail, Refund, and GetById operations.
    public async Task<Payment?> GetByIdAsync(Guid paymentId)
        => await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    // All payments for a student — most recent first (purchase history).
    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId)
        => await _context.Payments
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    // All payments for a course — most recent first (instructor/admin dashboard).
    public async Task<IEnumerable<Payment>> GetByCourseIdAsync(Guid courseId)
        => await _context.Payments
            .Where(p => p.CourseId == courseId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    // Composite lookup: find if this student has a payment for this course.
    // Used by InitiatePayment for the duplicate purchase check.
    // Returns null if no payment exists (safe to proceed).
    public async Task<Payment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        => await _context.Payments
            .FirstOrDefaultAsync(p => p.StudentId == studentId
                                   && p.CourseId == courseId);

    // Marks entity as Added — INSERT runs when SaveChangesAsync() is called.
    public async Task AddAsync(Payment payment)
        => await _context.Payments.AddAsync(payment);

    // Commits all pending changes (INSERT new payments, UPDATE status changes).
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}