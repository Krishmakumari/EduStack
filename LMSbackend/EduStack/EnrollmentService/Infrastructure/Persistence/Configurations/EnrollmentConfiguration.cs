// EnrollmentConfiguration — EF Fluent API table rules for the Enrollments table.
// • Status stored as string for DB readability (e.g., "Active" not "0").
// • Unique composite index on (StudentId, CourseId) — prevents double enrollment at DB level.
// • Cascade delete: deleting an Enrollment deletes all its LessonProgress records.

using EnrollmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnrollmentService.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.EnrollmentId);    // PK

        builder.Property(e => e.StudentName)
            .IsRequired()
            .HasMaxLength(100);                  // reasonable cap for student names

        builder.Property(e => e.CourseTitle)
            .IsRequired()
            .HasMaxLength(200);                  // same length as Course Service's CourseTitle

        builder.Property(e => e.PricePaid)
            .HasColumnType("decimal(18,2)");     // currency precision

        // Store enum as string ("Active", "Completed", "Cancelled") not integer.
        // Makes SQL queries easier to understand: WHERE Status = 'Active' not Status = 0.
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // UNIQUE CONSTRAINT — second layer of duplicate enrollment prevention.
        // Application layer checks too, but this handles race conditions.
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // CASCADE DELETE — when an Enrollment is deleted, all LessonProgress rows are deleted.
        // Prevents orphaned progress records from piling up in the DB.
        builder.HasMany(e => e.LessonProgresses)
            .WithOne(p => p.Enrollment)
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}