// UserAnswerConfiguration — EF connection rules for UserAnswer entity.
// • Limits `SelectedAnswer` size, optimizing for small text inputs.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> builder)
    {
        builder.HasKey(ua => ua.AnswerId);

        builder.Property(ua => ua.SelectedAnswer)
            .IsRequired()
            .HasMaxLength(500);
    }
}