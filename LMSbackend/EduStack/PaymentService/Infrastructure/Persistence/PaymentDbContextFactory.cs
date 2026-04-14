// PaymentDbContextFactory — Design-time factory for EF Core migrations CLI.
// • Used by `dotnet ef migrations add` — no DI container at design time.
// • Reads connection string from appsettings.json (unlike other services that hardcode it).
// • Builds IConfiguration from appsettings.json at the current working directory.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PaymentService.Infrastructure.Persistence;

public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        // Build config manually — no DI available at design time.
        // Reads from appsettings.json in the project directory.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();

        // Uses the same "PaymentConnection" key as Program.cs — consistent naming.
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("PaymentConnection"));

        return new PaymentDbContext(optionsBuilder.Options);
    }
}