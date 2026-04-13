// CertificateDbContext — EF Core gateway to the Certificate database.
// • Single table: Certificates (one entity, simple service).
// • Auto-discovers Fluent API config (CertificateConfiguration) via reflection.

using Microsoft.EntityFrameworkCore;
using CertificateService.Domain.Entities;

namespace CertificateService.Infrastructure.Persistence;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options)
        : base(options)
    {
    }

    // Single table — records certificate metadata (ID, user, course, file path).
    public DbSet<Certificate> Certificates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers CertificateConfiguration in the same assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CertificateDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}