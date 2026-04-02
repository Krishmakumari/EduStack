using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly AuthDbContext _context;

    public EmailVerificationRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerification verification)
        => await _context.EmailVerifications.AddAsync(verification);

    public async Task<EmailVerification?> GetByTokenAsync(string token)
        => await _context.EmailVerifications
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == token);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}