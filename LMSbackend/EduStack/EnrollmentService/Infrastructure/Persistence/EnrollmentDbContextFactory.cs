using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EnrollmentService.Infrastructure.Persistence;

public class EnrollmentDbContextFactory : IDesignTimeDbContextFactory<EnrollmentDbContext>
{
    public EnrollmentDbContext CreateDbContext(string[] args)
    {
        var startDir = Directory.GetCurrentDirectory();

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(startDir)
            .AddJsonFile("appsettings.json", optional: true);

        // search up to 6 parent levels for an EnrollmentService/appsettings.json
        string? enrollmentSettingsPath = null;
        var dirInfo = new DirectoryInfo(startDir);
        for (int i = 0; i < 6 && dirInfo != null; i++)
        {
            var candidate = Path.Combine(dirInfo.FullName, "EnrollmentService", "appsettings.json");
            if (File.Exists(candidate))
            {
                enrollmentSettingsPath = candidate;
                break;
            }

            if (string.Equals(dirInfo.Name, "EnrollmentService", StringComparison.OrdinalIgnoreCase))
            {
                candidate = Path.Combine(dirInfo.FullName, "appsettings.json");
                if (File.Exists(candidate))
                {
                    enrollmentSettingsPath = candidate;
                    break;
                }
            }

            dirInfo = dirInfo.Parent;
        }

        if (!string.IsNullOrEmpty(enrollmentSettingsPath))
        {
            configBuilder.AddJsonFile(enrollmentSettingsPath, optional: true);
        }

        configBuilder.AddEnvironmentVariables();
        var configuration = configBuilder.Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Ensure it is defined in EnrollmentService/appsettings.json or environment variables.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<EnrollmentDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new EnrollmentDbContext(optionsBuilder.Options);
    }
}