// QuizAttemptConfiguration — EF configuration for QuizAttempt entity.
// • Enum-as-string for Status (InProgress/Passed/Failed).
// • decimal(5,2) for exact score representation (supports fractional grades if needed).
// • Cascade delete to UserAnswers (if attempt is deleted, its answers are erased).

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasKey(a => a.AttemptId);

        builder.Property(a => a.Score)
            .HasColumnType("decimal(5,2)");

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.Attempt)
            .HasForeignKey(ans => ans.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}