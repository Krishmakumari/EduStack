namespace AuthService.Application.DTOs.Requests;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}