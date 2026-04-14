// LessonProgressConfiguration — EF Fluent API table rules for the LessonProgresses table.
// • Unique composite index (EnrollmentId + LessonId) — one progress record per lesson per enrollment.
// • Prevents duplicate completion records at the database level (application checks too).

using EnrollmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnrollmentService.Infrastructure.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.HasKey(p => p.LessonProgressId);    // PK

        // A student can only have ONE progress record per lesson per enrollment.
        // This is the DB-level guard against duplicate completion records.
        builder.HasIndex(p => new { p.EnrollmentId, p.LessonId })
            .IsUnique();
    }
}