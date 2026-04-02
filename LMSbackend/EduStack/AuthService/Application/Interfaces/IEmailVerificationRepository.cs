using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IEmailVerificationRepository
{
    Task AddAsync(EmailVerification verification);
    Task<EmailVerification?> GetByTokenAsync(string token);
    Task SaveChangesAsync();
}