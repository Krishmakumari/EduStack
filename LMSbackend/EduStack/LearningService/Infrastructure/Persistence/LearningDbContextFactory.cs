// LearningDbContextFactory — Design-time factory for EF Core migrations CLI.
// • Used by `dotnet ef migrations add` — no DI container available at design time.
// • NOTE: Connection string is hardcoded here (unlike other services that read appsettings).
//   This is acceptable ONLY for the migrations factory — production uses appsettings + DI.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LearningService.Infrastructure.Persistence;

public class LearningDbContextFactory : IDesignTimeDbContextFactory<LearningDbContext>
{
    public LearningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LearningDbContext>();

        // Hardcoded connection string for design-time use only.
        // In production, the connection string comes from appsettings.json via DI in Program.cs.
        optionsBuilder.UseSqlServer(
            "Server=.\\sqlexpress;Database=EduStackLearning;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LearningDbContext(optionsBuilder.Options);
    }
}
