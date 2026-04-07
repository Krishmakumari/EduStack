using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CourseService.Infrastructure.Persistence;

public class CourseDbContextFactory : IDesignTimeDbContextFactory<CourseDbContext>
{
    public CourseDbContext CreateDbContext(string[] args)
    {
        var startDir = Directory.GetCurrentDirectory();

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(startDir)
            .AddJsonFile("appsettings.json", optional: true);

        // search up to 6 parent levels for a CourseService/appsettings.json file
        string? courseSettingsPath = null;
        var dirInfo = new DirectoryInfo(startDir);
        for (int i = 0; i < 6 && dirInfo != null; i++)
        {
            var candidate = Path.Combine(dirInfo.FullName, "CourseService", "appsettings.json");
            if (File.Exists(candidate))
            {
                courseSettingsPath = candidate;
                break;
            }

            // if this directory itself is CourseService, check its appsettings
            if (string.Equals(dirInfo.Name, "CourseService", StringComparison.OrdinalIgnoreCase))
            {
                candidate = Path.Combine(dirInfo.FullName, "appsettings.json");
                if (File.Exists(candidate))
                {
                    courseSettingsPath = candidate;
                    break;
                }
            }

            dirInfo = dirInfo.Parent;
        }

        if (!string.IsNullOrEmpty(courseSettingsPath))
        {
            configBuilder.AddJsonFile(courseSettingsPath, optional: true);
        }

        configBuilder.AddEnvironmentVariables();
        var configuration = configBuilder.Build();

        var connectionString = configuration.GetConnectionString("CourseConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // explicit environment variable fallback
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CourseConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'CourseConnection' was not found. Ensure it is defined in CourseService/appsettings.json or environment variables.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CourseDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CourseDbContext(optionsBuilder.Options);

    }

}