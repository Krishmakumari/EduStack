// CourseDbContext — EF Core gateway to the Course database.
// • 3 tables: Courses, Sections, Lessons (database-per-service pattern).
// • Auto-discovers Fluent API configs (CourseConfiguration, etc.) via reflection.

using CourseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CourseService.Infrastructure.Persistence;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}