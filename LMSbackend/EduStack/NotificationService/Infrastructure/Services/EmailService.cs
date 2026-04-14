// EmailService — Sends emails via Gmail SMTP using MailKit/MimeKit (Infrastructure Layer).
// • MailKit is the recommended .NET email library (built-in SmtpClient is obsolete).
// • Gmail SMTP: port 587 (STARTTLS — starts plain, upgrades to TLS).
// • Auth uses Gmail App Password (not account password — required by Google for SMTP).
// • New SmtpClient per email — acceptable for low-volume; pool connections at high scale.

using MailKit.Net.Smtp;
using MimeKit;

namespace NotificationService.Infrastructure.Services;

public class EmailService
{
    // Configuration injected via DI — reads Email:From and Email:AppPassword from appsettings.
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    // SendEmailAsync — constructs a MIME email and sends it via Gmail SMTP.
    // Steps: Build MimeMessage → Connect → Authenticate → Send → Disconnect.
    // Called by both NotificationAppService (HTTP path) and RabbitMqConsumer (async path).
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // MimeMessage is MimeKit's email object — supports plain text and HTML bodies.
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("LMS", _config["Email:From"]));  // sender display name
        email.To.Add(MailboxAddress.Parse(to));   // recipient email address
        email.Subject = subject;

        // Plain text body — could switch to "html" for rich HTML email templates.
        email.Body = new TextPart("plain") { Text = body };

        // `using` ensures smtp.Dispose() is called — closes connection even on exception.
        using var smtp = new SmtpClient();

        // Port 587 with false = STARTTLS (not immediate SSL).
        // Negotiates TLS after connecting on the plain port.
        await smtp.ConnectAsync("smtp.gmail.com", 587, false);

        // Gmail App Password — generated in Google Account > Security > App Passwords.
        // Required because Google blocks regular password SMTP access for security.
        await smtp.AuthenticateAsync(_config["Email:From"], _config["Email:AppPassword"]);

        await smtp.SendAsync(email);

        // true = send QUIT command to the server before closing TCP connection.
        await smtp.DisconnectAsync(true);
    }
}
