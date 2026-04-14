// EnrollmentDbContext — EF Core gateway to the Enrollment database.
// • 2 tables: Enrollments, LessonProgresses (database-per-service pattern).
// • Auto-discovers Fluent API configs (EnrollmentConfiguration, LessonProgressConfiguration).

using EnrollmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EnrollmentService.Infrastructure.Persistence;

public class EnrollmentDbContext : DbContext
{
    public EnrollmentDbContext(DbContextOptions<EnrollmentDbContext> options)
        : base(options) { }

    // Enrollments table — one row per student-course relationship.
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    // LessonProgresses table — one row per completed lesson per enrollment.
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers EnrollmentConfiguration and LessonProgressConfiguration
        // from the same assembly — cleaner than calling Configure() manually.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}