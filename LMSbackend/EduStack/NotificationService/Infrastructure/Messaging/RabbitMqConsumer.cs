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
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(EmailService emailService, RabbitMqConnection rabbitMqConnection)
    {
        _emailService = emailService;
        _rabbitMqConnection = rabbitMqConnection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await _rabbitMqConnection.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await DeclareAndConsumeAsync("certificate_queue", HandleCertificateAsync);
        await DeclareAndConsumeAsync("enrollment_queue", HandleEnrollmentAsync);
        await DeclareAndConsumeAsync("quiz_queue", HandleQuizAsync);
    }

    private async Task DeclareAndConsumeAsync(string queue, Func<string, Task> handler)
    {
        await _channel!.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            await handler(json);
        };

        await _channel.BasicConsumeAsync(queue, autoAck: true, consumer: consumer);
    }

    private async Task HandleCertificateAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<CertificateGeneratedEvent>(json);
        if (evt is null) return;
        await _emailService.SendEmailAsync(
            evt.Email,
            "Certificate Ready 🎉",
            $"Congrats! Your certificate for '{evt.CourseTitle}' is ready.");
    }

    private async Task HandleEnrollmentAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<EnrollmentCompletedEvent>(json);
        if (evt is null) return;
        await _emailService.SendEmailAsync(
            evt.Email,
            "Enrollment Confirmed ✅",
            $"You have successfully enrolled in '{evt.CourseTitle}'.");
    }

    private async Task HandleQuizAsync(string json)
    {
        var evt = JsonSerializer.Deserialize<QuizResultEvent>(json);
        if (evt is null) return;
        var result = evt.Passed ? "passed 🎉" : "not passed ❌";
        await _emailService.SendEmailAsync(
            evt.Email,
            "Quiz Result",
            $"Your quiz result is in — you {result}.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}
