using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqConnection
{
    private readonly IConfiguration _config;

    public RabbitMqConnection(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost"
        };

        return await factory.CreateConnectionAsync();
    }
}
