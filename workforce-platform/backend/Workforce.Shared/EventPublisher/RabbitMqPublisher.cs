using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workforce.Shared.Events;

namespace Workforce.Shared.EventPublisher;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
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
            _logger.LogInformation("RabbitMQ connection initialized for exchange: {ExchangeName}", exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public Task<string> PublishEventAsync(AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default)
    {
        if (_channel == null || _disposed)
            throw new InvalidOperationException("RabbitMQ connection is not available");

        var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
        
        // Use provided eventId or generate one
        eventId ??= Guid.NewGuid().ToString();
        
        // Convert enum to routing key string
        var routingKey = eventType.ToRoutingKey();
        
        var eventPayload = new
        {
            EventId = eventId,
            EventType = routingKey,
            Timestamp = DateTime.UtcNow,
            Data = eventData
        };

        var message = JsonSerializer.Serialize(eventPayload);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        _logger.LogDebug("Published event {EventType} ({RoutingKey}) with ID {EventId}", eventType, routingKey, eventId);
        return Task.FromResult(eventId);
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
