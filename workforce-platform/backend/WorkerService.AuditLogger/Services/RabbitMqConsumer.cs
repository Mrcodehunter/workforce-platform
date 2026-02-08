using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace WorkerService.AuditLogger.Services;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public RabbitMqConsumer(IConfiguration configuration, IAuditLogService auditLogService, ILogger<RabbitMqConsumer> logger)
    {
        _configuration = configuration;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var host = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
            var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var username = _configuration["RabbitMQ:Username"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "audit.queue";

            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // Declare queue
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            // Bind queue to exchange with routing keys
            var routingKeys = _configuration.GetSection("RabbitMQ:RoutingKeys").Get<string[]>() 
                ?? new[] { "#" }; // Default to all events

            foreach (var routingKey in routingKeys)
            {
                _channel.QueueBind(queueName, exchangeName, routingKey);
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Received event: {RoutingKey}", routingKey);

                    // Parse event payload (structure: { EventId, EventType, Timestamp, Data })
                    var eventPayload = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
                    var eventId = eventPayload?.ContainsKey("EventId") == true 
                        ? eventPayload["EventId"]?.ToString() 
                        : Guid.NewGuid().ToString();

                    // Pass the entire payload to the audit service so it can extract entity info
                    await _auditLogService.LogEventAsync(
                        eventId ?? Guid.NewGuid().ToString(), 
                        routingKey, 
                        eventPayload ?? new Dictionary<string, object>());

                    _channel?.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel?.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queueName, autoAck: false, consumer);
            _logger.LogInformation("RabbitMQ consumer started for queue: {QueueName}", queueName);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting RabbitMQ consumer");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("RabbitMQ consumer stopped");
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
