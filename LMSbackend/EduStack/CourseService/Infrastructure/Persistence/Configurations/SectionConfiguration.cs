// SectionConfiguration — EF Fluent API table rules for the Sections table.
// • Cascade delete: deleting a Section deletes all its Lessons.

using CourseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseService.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(s => s.SectionId);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(s => s.Lessons)
            .WithOne(l => l.Section)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}