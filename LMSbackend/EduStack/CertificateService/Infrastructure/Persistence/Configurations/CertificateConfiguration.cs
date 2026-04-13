// CertificateConfiguration — EF Fluent API table rules for the Certificates table.
// • UserName, CourseTitle, FilePath are all required — no certificate without these.
// • MaxLength 200 for names to prevent unbounded string columns.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CertificateService.Domain.Entities;

namespace CertificateService.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.CertificateId);        // PK

        builder.Property(c => c.UserName)
            .IsRequired()
            .HasMaxLength(200);                       // student name max 200 chars

        builder.Property(c => c.CourseTitle)
            .IsRequired()
            .HasMaxLength(200);                       // course title max 200 chars

        builder.Property(c => c.FilePath)
            .IsRequired();                            // must always have a file path
    }
}