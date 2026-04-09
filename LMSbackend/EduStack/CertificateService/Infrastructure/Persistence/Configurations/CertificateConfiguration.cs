using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CertificateService.Domain.Entities;

namespace CertificateService.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.CertificateId);

        builder.Property(c => c.UserName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CourseTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.FilePath)
            .IsRequired();
    }
}