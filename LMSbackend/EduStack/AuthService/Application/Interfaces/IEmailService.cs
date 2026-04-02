namespace AuthService.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string token);
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp);
}