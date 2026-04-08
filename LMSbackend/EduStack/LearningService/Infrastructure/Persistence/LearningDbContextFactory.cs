using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LearningService.Infrastructure.Persistence;

public class LearningDbContextFactory : IDesignTimeDbContextFactory<LearningDbContext>
{
    public LearningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LearningDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=.\\sqlexpress;Database=EduStackLearning;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LearningDbContext(optionsBuilder.Options);
    }
}
