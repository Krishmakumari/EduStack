// CertificateDbContextFactory — Design-time factory for EF Core migrations CLI.
// • Used by `dotnet ef migrations add` when the app isn't running (no DI container).
// • Reads connection string from appsettings.json in the current directory.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CertificateService.Infrastructure.Persistence;

public class CertificateDbContextFactory
    : IDesignTimeDbContextFactory<CertificateDbContext>
{
    public CertificateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CertificateDbContext>();

        // Build configuration manually — no DI available during migration generation.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new CertificateDbContext(optionsBuilder.Options);
    }
}