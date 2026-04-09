using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasKey(a => a.AttemptId);

        builder.Property(a => a.Score).IsRequired();
        builder.Property(a => a.IsPassed).IsRequired();

        builder.Property(a => a.AttemptedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}