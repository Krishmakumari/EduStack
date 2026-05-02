using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        // Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Check if Admin exists
        var adminEmail = "admin@edustack.com";
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin == null)
        {
            // Create hardcoded Admin user
            // Password is "Admin@123" hashed using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            
            var admin = User.Create(
                "System Administrator", 
                adminEmail, 
                passwordHash, 
                UserRole.Admin
            );
            
            admin.MarkEmailVerified();

            context.Users.Add(admin);
            await context.SaveChangesAsync();
            
            Console.WriteLine("[AuthService] Hardcoded Admin user seeded successfully.");
        }
    }
}
