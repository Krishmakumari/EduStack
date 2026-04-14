// PaymentConfiguration — EF Fluent API table rules for the Payments table.
// • Status and Method stored as strings for SQL readability.
// • Cascade delete: deleting a Payment deletes its associated Refunds.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.PaymentId);   // PK

        builder.Property(p => p.StudentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CourseTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");   // standard currency precision

        // Store enum as string: "Completed" not 1 — readable SQL queries.
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Store method as string: "UPI" not 2 — readable SQL queries.
        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);    // gateway IDs can be long

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);    // allow detailed failure messages

        // CASCADE DELETE: when Payment is deleted, all its Refunds are deleted.
        // Prevents orphaned Refund rows from accumulating.
        builder.HasMany<Refund>()
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}