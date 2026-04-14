// PaymentDbContext — EF Core gateway to the Payment database.
// • 2 tables: Payments and Refunds (database-per-service pattern).
// • Auto-discovers PaymentConfiguration and RefundConfiguration via assembly scan.

using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using System.Reflection;

namespace PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    // Payments table — one row per student course purchase.
    public DbSet<Payment> Payments => Set<Payment>();

    // Refunds table — one row per refunded payment (linked via PaymentId FK).
    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers PaymentConfiguration and RefundConfiguration.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}