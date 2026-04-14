// RefundConfiguration — EF Fluent API table rules for the Refunds table.
// • Reason required and max-length capped — prevents unbounded text storage.
// • Amount as decimal(18,2) for currency precision.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.RefundId);    // PK

        // Reason is required — every refund must have a stated reason for audit trail.
        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)"); // standard currency precision

        builder.Property(r => r.TransactionId)
            .HasMaxLength(200);              // gateway refund reference ID
    }
}