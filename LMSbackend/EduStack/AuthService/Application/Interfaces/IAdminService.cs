using AuthService.Application.DTOs.Responses;

namespace AuthService.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<MessageResponse> BanUserAsync(Guid userId);
    Task<MessageResponse> UnbanUserAsync(Guid userId);
    Task<MessageResponse> UpdateUserRoleAsync(Guid userId, string newRole);
}
