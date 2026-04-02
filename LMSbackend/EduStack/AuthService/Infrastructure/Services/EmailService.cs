using AuthService.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string token)
    {
        var verifyUrl = $"{_config["App:BaseUrl"]}/api/auth/verify-email?token={token}";

        var body = $"""
            <h2>Hello {fullName},</h2>
            <p>Please verify your email by clicking the link below:</p>
            <a href="{verifyUrl}">Verify Email</a>
            <p>This link expires in 60 minutes.</p>
            """;

        await SendAsync(toEmail, fullName, "Verify Your Email — EduLearn", body);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp)
    {
        var body = $"""
            <h2>Hello {fullName},</h2>
            <p>Your OTP for password reset is:</p>
            <h1 style="letter-spacing:8px;">{otp}</h1>
            <p>This OTP expires in 15 minutes. Do not share it with anyone.</p>
            """;

        await SendAsync(toEmail, fullName, "Password Reset OTP — EduLearn", body);
    }

    // ─── Private helper ───────────────────────────────────────────────────────
    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _config["Email:SenderName"],
            _config["Email:SenderEmail"]));

        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _config["Email:Host"],
            int.Parse(_config["Email:Port"]!),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _config["Email:Username"],
            _config["Email:Password"]);

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}