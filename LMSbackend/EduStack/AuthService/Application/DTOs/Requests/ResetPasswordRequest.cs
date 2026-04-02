namespace AuthService.Application.DTOs.Requests;

public class ResetPasswordRequest
{
    public string Email { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}