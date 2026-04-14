// RabbitMqConnection — Factory for creating RabbitMQ connections (Infrastructure Layer).
// • Registered as Singleton — one connection shared for the app's lifetime.
// • ConnectionFactory is the RabbitMQ.Client entry point; HostName from appsettings.
// • Called once by RabbitMqConsumer.StartAsync() at application startup.

using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqConnection
{
    private readonly IConfiguration _config;

    public RabbitMqConnection(IConfiguration config)
    {
        _config = config;
    }

    // CreateConnectionAsync — connects to the RabbitMQ broker.
    // HostName from appsettings["RabbitMQ:Host"] — defaults to "localhost" if not set.
    // In production: add Username, Password, VirtualHost, and TLS settings.
    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            // RabbitMQ:Host from appsettings.json — e.g., "localhost" for local dev,
            // or container name (e.g., "rabbitmq") when running in Docker Compose.
            HostName = _config["RabbitMQ:Host"] ?? "localhost"
        };

        return await factory.CreateConnectionAsync();
    }
}
