// QuizDbContext — EF Core gateway to the Quiz Service database.
// • Manages quizzes, questions, student attempts, and answers.
// • Uses Reflection to load all configurations defined in this assembly.

using Microsoft.EntityFrameworkCore;
using QuizService.Domain.Entities;
using System.Reflection;

namespace QuizService.Infrastructure.Persistence;

public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options)
        : base(options)
    {
    }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}