// LessonProgressConfiguration — EF Fluent API table rules for LessonProgresses table.
// • Unique composite index on (UserId, CourseId, LessonId) prevents duplicate records.
// • One progress record per student per lesson — upsert pattern in service layer.
// • All three ID fields are required (system-assigned, always present).

using LearningService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearningService.Infrastructure.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.HasKey(x => x.Id);   // PK

        // All three IDs are required — no orphaned progress records.
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CourseId).IsRequired();
        builder.Property(x => x.LessonId).IsRequired();

        builder.Property(x => x.WatchedSeconds)
            .IsRequired();

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.LastAccessedAt)
            .IsRequired();

        // Unique composite index — one progress record per user+course+lesson combination.
        // Prevents duplicate records and enables fast 3-key lookups.
        // Matches the query pattern in ProgressRepository.GetAsync().
        builder.HasIndex(x => new { x.UserId, x.CourseId, x.LessonId })
            .IsUnique();
    }
}