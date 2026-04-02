using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AuthDbContext _context;

    public PasswordResetRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordReset reset)
        => await _context.PasswordResets.AddAsync(reset);

    public async Task<PasswordReset?> GetLatestByUserIdAsync(Guid userId)
        => await _context.PasswordResets
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}