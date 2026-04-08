using LearningService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearningService.Infrastructure.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CourseId).IsRequired();
        builder.Property(x => x.LessonId).IsRequired();

        builder.Property(x => x.WatchedSeconds)
            .IsRequired();

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.LastAccessedAt)
            .IsRequired();

        // 🔥 Prevent duplicate progress per user+lesson
        builder.HasIndex(x => new { x.UserId, x.CourseId, x.LessonId })
            .IsUnique();
    }
}