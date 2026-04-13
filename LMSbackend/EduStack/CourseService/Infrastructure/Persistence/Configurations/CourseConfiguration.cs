// CourseConfiguration — EF Fluent API table rules for the Courses table.
// • Enums (Level, Status) stored as strings for DB readability.
// • Price uses decimal(18,2) for currency precision.
// • Cascade delete: deleting a Course deletes all its Sections (and their Lessons).

using CourseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseService.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.CourseId);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.ThumbnailUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Level)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.InstructorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(c => c.Sections)
            .WithOne(s => s.Course)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}