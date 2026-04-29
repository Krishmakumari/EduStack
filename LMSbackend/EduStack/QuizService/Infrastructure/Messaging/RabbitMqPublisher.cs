using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace QuizService.Infrastructure.Messaging;

public class RabbitMqPublisher
{
    private readonly IConfiguration _config;

    public RabbitMqPublisher(IConfiguration config)
    {
        _config = config;
    }

    public async Task PublishAsync<T>(string queue, T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queue,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: string.Empty,
                             routingKey: queue,
                             body: body);
    }
}
