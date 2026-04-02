namespace AuthService.Application.DTOs.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}