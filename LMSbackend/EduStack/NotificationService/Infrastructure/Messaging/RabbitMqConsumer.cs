// RabbitMqConsumer — Background service that listens to RabbitMQ queues and sends emails.
// • Implements IHostedService — runs for the entire application lifetime (not per-request).
// • Consumes 3 queues simultaneously: certificate_queue, enrollment_queue, quiz_queue.
// • On message arrival: deserialize JSON event → call EmailService → send email.
// • autoAck: true — message acknowledged automatically on receipt (before processing).
//   Improvement for production: use autoAck:false + manual BasicAck for reliability.

using System.Text;
using System.Text.Json;
using NotificationService.Domain.Events;
using NotificationService.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqConsumer : IHostedService
{
    private readonly EmailService _emailService;
    private readonly RabbitMqConnection _rabbitMqConnection;

    // Connection and channel are created once in StartAsync and reused until StopAsync.
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(EmailService emailService, RabbitMqConnection rabbitMqConnection)
    {
        _emailService = emailService;
        _rabbitMqConnection = rabbitMqConnection;
    }

    // StartAsync — called automatically when the application starts.
    // Creates one connection and one channel, then declares all 3 queues.
    // Each queue gets its own handler delegate — messages are processed concurrently.
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await _rabbitMqConnection.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Set up all 3 queues with their respective handlers.
        // DeclareAndConsumeAsync is a reusable factory that wires queue → handler.
        await DeclareAndConsumeAsync("certificate_queue", HandleCertificateAsync);
        await DeclareAndConsumeAsync("enrollment_queue", HandleEnrollmentAsync);
        await DeclareAndConsumeAsync("quiz_queue", HandleQuizAsync);
    }

    // DeclareAndConsumeAsync — reusable factory to set up a queue + consumer.
    // Avoids duplicating QueueDeclare + Consumer wiring 3 times.
    // QueueDeclare is idempotent — safe to call even if queue already exists.
    private async Task DeclareAndConsumeAsync(string queue, Func<string, Task> handler)
    {
        // Declare the queue with durable:false — queue doesn't survive broker restart.
        // In production: use durable:true so messages survive RabbitMQ restarts.
        await _channel!.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        // Wire the received event: decode bytes → UTF-8 string → pass to handler.
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            await handler(json);
        };

        // Start consuming. autoAck: true = message is acknowledged immediately on receipt.
        // Risk: if handler (email send) throws, message is already gone from queue.
        // Production fix: autoAck: false + BasicAck after successful send.
        await _channel.BasicConsumeAsync(queue, autoAck: true, consumer: consumer);
    }

    // HandleCertificateAsync — triggered when a certificate is generated.
    // Deserializes CertificateGeneratedEvent and sends congratulations email.
    private async Task HandleCertificateAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<CertificateGeneratedEvent>(json);
        if (evt is null) return;   // guard: discard malformed/null messages
        await _emailService.SendEmailAsync(
            evt.Email,
            "Certificate Ready 🎉",
            $"Congrats! Your certificate for '{evt.CourseTitle}' is ready.");
    }

    // HandleEnrollmentAsync — triggered when a student completes enrollment.
    // Deserializes EnrollmentCompletedEvent and sends confirmation email.
    private async Task HandleEnrollmentAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<EnrollmentCompletedEvent>(json);
        if (evt is null) return;
        await _emailService.SendEmailAsync(
            evt.Email,
            "Enrollment Confirmed ✅",
            $"You have successfully enrolled in '{evt.CourseTitle}'.");
    }

    // HandleQuizAsync — triggered when quiz results are available.
    // Deserializes QuizResultEvent and sends pass/fail notification email.
    private async Task HandleQuizAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<QuizResultEvent>(json);
        if (evt is null) return;

        // Pass/fail message changes based on quiz result.
        var result = evt.Passed ? "passed 🎉" : "not passed ❌";
        await _emailService.SendEmailAsync(
            evt.Email,
            "Quiz Result",
            $"Your quiz result is in — you {result}.");
    }

    // StopAsync — called automatically when the application shuts down.
    // Closes channel and connection gracefully (no abrupt disconnection from broker).
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}
