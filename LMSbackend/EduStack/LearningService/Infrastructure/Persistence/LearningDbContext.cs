// LearningDbContext — EF Core gateway to the Learning database.
// • Single table: LessonProgresses (one row per user + course + lesson).
// • Auto-discovers LessonProgressConfiguration via ApplyConfigurationsFromAssembly.

using LearningService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LearningService.Infrastructure.Persistence;

public class LearningDbContext : DbContext
{
    public LearningDbContext(DbContextOptions<LearningDbContext> options)
        : base(options)
    {
    }

    // One table: LessonProgresses — stores watch position per student per lesson.
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers LessonProgressConfiguration — applies PK, indexes, constraints.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}