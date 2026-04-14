// QuizDbContextFactory — Design-time factory for EF Core migrations CLI.
// • Required to run EF CLI commands. Loads appsettings.json dynamically.
// • Similar logic across services, essential for `dotnet ef migrations add`.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QuizService.Infrastructure.Persistence;

public class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
{
    public QuizDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("QuizConnection"));

        return new QuizDbContext(optionsBuilder.Options);
    }
}