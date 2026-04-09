using Microsoft.EntityFrameworkCore;
using CertificateService.Domain.Entities;

namespace CertificateService.Infrastructure.Persistence;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Certificate> Certificates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CertificateDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}