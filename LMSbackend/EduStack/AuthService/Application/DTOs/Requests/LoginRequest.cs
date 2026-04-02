namespace AuthService.Application.DTOs.Requests;

public class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    //public string? DeviceInfo { get; set; }
}