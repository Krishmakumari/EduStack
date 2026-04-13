// AuthDbContext — EF Core gateway to the Auth database.
// • Each DbSet<T> maps to a table; EF Core translates LINQ to SQL.
// • Database-per-Service pattern: Auth DB only has auth-related tables.
// • Auto-discovers all IEntityTypeConfiguration<T> classes via reflection.

using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // ── Table Mappings ──────────────────────────────────────────────────────
    // Each DbSet<T> = one table in the Auth database.
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scans the current assembly for all IEntityTypeConfiguration<T> classes
        // (e.g., UserConfiguration, RefreshTokenConfiguration) and applies them.
        // This keeps table definitions (column lengths, indexes, FK rules) in
        // separate, organized configuration files rather than cluttering this class.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}