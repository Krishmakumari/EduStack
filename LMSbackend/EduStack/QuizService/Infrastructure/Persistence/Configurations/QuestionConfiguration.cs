using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.QuestionId);

        builder.Property(q => q.Text).IsRequired();

        builder.Property(q => q.OptionA).IsRequired();
        builder.Property(q => q.OptionB).IsRequired();
        builder.Property(q => q.OptionC).IsRequired();
        builder.Property(q => q.OptionD).IsRequired();

        builder.Property(q => q.CorrectAnswer).IsRequired();
    }
}