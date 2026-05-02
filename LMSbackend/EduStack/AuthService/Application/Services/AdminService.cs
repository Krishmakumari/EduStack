using AuthService.Application.DTOs.Responses;
using AuthService.Application.Interfaces;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;

    public AdminService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(u => new UserResponseDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsBanned = u.IsBanned,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<MessageResponse> BanUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        user.Ban(); // Custom method on User entity
        await _userRepo.SaveChangesAsync();

        return new MessageResponse($"User {user.Email} has been banned.");
    }

    public async Task<MessageResponse> UnbanUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        user.Unban(); // Custom method on User entity
        await _userRepo.SaveChangesAsync();

        return new MessageResponse($"User {user.Email} has been unbanned.");
    }

    public async Task<MessageResponse> UpdateUserRoleAsync(Guid userId, string newRole)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        if (!Enum.TryParse<UserRole>(newRole, true, out var role))
            throw new DomainException("Invalid role specified.");

        user.UpdateRole(role); // Custom method on User entity
        await _userRepo.SaveChangesAsync();

        return new MessageResponse($"User {user.Email} role updated to {role}.");
    }
}
