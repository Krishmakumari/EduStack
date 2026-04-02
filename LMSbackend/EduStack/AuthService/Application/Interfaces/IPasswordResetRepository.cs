using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IPasswordResetRepository
{
    Task AddAsync(PasswordReset reset);
    Task<PasswordReset?> GetLatestByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}