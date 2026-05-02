using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}