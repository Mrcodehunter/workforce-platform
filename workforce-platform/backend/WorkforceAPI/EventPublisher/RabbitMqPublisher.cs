using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WorkforceAPI.EventPublisher;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        var host = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
        var username = _configuration["RabbitMQ:Username"] ?? "guest";
        var password = _configuration["RabbitMQ:Password"] ?? "guest";
        var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
        var exchangeType = _configuration["RabbitMQ:ExchangeType"] ?? "topic";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);
    }

    public Task PublishEventAsync(string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        if (_channel == null || _disposed)
            throw new InvalidOperationException("RabbitMQ connection is not available");

        var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
        var eventPayload = new
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Data = eventData
        };

        var message = JsonSerializer.Serialize(eventPayload);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: eventType,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
