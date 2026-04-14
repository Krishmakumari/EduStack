// QuestionConfiguration — EF connection rules for Question entity.
// • Type enum is saved as a string to make DB legible.
// • CorrectAnswer is stored with fixed casing.
// • Options can be extremely large JSON strings, so MaxLength is bumped to 1000.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.QuestionId);

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(q => q.CorrectAnswer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Options)
            .HasMaxLength(1000);
    }
}