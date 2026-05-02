using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("{userId:guid}/ban")]
    public async Task<IActionResult> BanUser(Guid userId)
    {
        var result = await _adminService.BanUserAsync(userId);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/unban")]
    public async Task<IActionResult> UnbanUser(Guid userId)
    {
        var result = await _adminService.UnbanUserAsync(userId);
        return Ok(result);
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] string newRole)
    {
        var result = await _adminService.UpdateUserRoleAsync(userId, newRole);
        return Ok(result);
    }
}
