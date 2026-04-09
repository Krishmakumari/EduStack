using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizService.Domain.Entities;

namespace QuizService.Infrastructure.Persistence.Configurations;

public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SelectedAnswer)
            .IsRequired();
    }
}