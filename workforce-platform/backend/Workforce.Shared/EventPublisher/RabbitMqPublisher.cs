using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workforce.Shared.Events;

namespace Workforce.Shared.EventPublisher;

/// <summary>
/// RabbitMQ event publisher implementation
/// Publishes domain events to RabbitMQ topic exchange for consumption by worker services
/// </summary>
/// <remarks>
/// This class manages a single RabbitMQ connection and channel for publishing events.
/// The connection is established during construction and reused for all event publications.
/// 
/// Events are published to a durable topic exchange, allowing multiple workers to subscribe
/// to different event patterns using routing keys (e.g., "employee.*", "project.member.*").
/// 
/// The event payload includes EventId, EventType (routing key), Timestamp, and Data.
/// The EventId is used to correlate events with before/after snapshots stored in Redis.
/// </remarks>
public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of RabbitMqPublisher
    /// </summary>
    /// <param name="configuration">Configuration to read RabbitMQ settings from</param>
    /// <param name="logger">Logger for connection and publishing events</param>
    /// <exception cref="Exception">Thrown if RabbitMQ connection fails during initialization</exception>
    /// <remarks>
    /// The connection is established immediately during construction. This ensures
    /// connection issues are discovered early rather than on first event publication.
    /// </remarks>
    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
        InitializeConnection();
    }

    /// <summary>
    /// Initializes the RabbitMQ connection and channel
    /// </summary>
    /// <exception cref="Exception">Thrown if connection or exchange declaration fails</exception>
    /// <remarks>
    /// This method:
    /// 1. Reads connection settings from configuration (with defaults for Docker/local environments)
    /// 2. Creates a connection to RabbitMQ
    /// 3. Opens a channel for publishing
    /// 4. Declares the topic exchange as durable (survives broker restarts)
    /// 
    /// The exchange is declared as durable to ensure events aren't lost if RabbitMQ restarts.
    /// </remarks>
    private void InitializeConnection()
    {
        try
        {
            // Read configuration with environment-aware defaults
            // Development: typically "localhost", Production: "rabbitmq" (Docker service name)
            var host = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
            var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var username = _configuration["RabbitMQ:Username"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
            var exchangeType = _configuration["RabbitMQ:ExchangeType"] ?? "topic";

            // Create connection factory with credentials
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };

            // Establish connection and open channel
            // Connection is long-lived and reused for all publications
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Declare exchange as durable to survive broker restarts
            // Topic exchange allows routing based on patterns (e.g., "employee.*", "project.member.*")
            _channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);
            _logger.LogInformation("RabbitMQ connection initialized for exchange: {ExchangeName}", exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    /// <summary>
    /// Publishes an audit event to RabbitMQ
    /// </summary>
    /// <param name="eventType">The type of audit event</param>
    /// <param name="eventData">The event payload data</param>
    /// <param name="eventId">Optional event ID (generated if not provided)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The event ID used for correlation</returns>
    /// <exception cref="InvalidOperationException">Thrown if connection is not available</exception>
    /// <remarks>
    /// This method:
    /// 1. Generates or uses provided eventId for correlation with Redis snapshots
    /// 2. Converts eventType enum to routing key string (e.g., "employee.created")
    /// 3. Creates event payload with EventId, EventType, Timestamp, and Data
    /// 4. Serializes payload to JSON
    /// 5. Publishes to topic exchange with routing key
    /// 
    /// The routing key allows workers to subscribe to specific event patterns.
    /// For example, the audit logger worker subscribes to "#" (all events).
    /// 
    /// IMPORTANT: This should be called AFTER storing snapshots in Redis to ensure
    /// data consistency. The eventId links the event to the Redis-stored snapshots.
    /// </remarks>
    public Task<string> PublishEventAsync(AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default)
    {
        // Validate connection state before publishing
        if (_channel == null || _disposed)
            throw new InvalidOperationException("RabbitMQ connection is not available");

        var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "workforce.events";
        
        // Use provided eventId or generate a new GUID
        // The eventId is critical for correlating the event with Redis-stored snapshots
        eventId ??= Guid.NewGuid().ToString();
        
        // Convert enum to routing key string (e.g., EmployeeCreated -> "employee.created")
        // This allows workers to subscribe using pattern matching
        var routingKey = eventType.ToRoutingKey();
        
        // Construct event payload with metadata
        // EventId: Used to retrieve before/after snapshots from Redis
        // EventType: The routing key string for worker subscription
        // Timestamp: When the event occurred (UTC)
        // Data: The actual event payload (entity-specific data)
        var eventPayload = new
        {
            EventId = eventId,
            EventType = routingKey,
            Timestamp = DateTime.UtcNow,
            Data = eventData
        };

        // Serialize to JSON and convert to bytes for RabbitMQ
        var message = JsonSerializer.Serialize(eventPayload);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish to topic exchange
        // The routing key determines which queues receive the message
        // Workers bind their queues to the exchange with routing key patterns
        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null, // No additional properties needed for audit events
            body: body);

        _logger.LogDebug("Published event {EventType} ({RoutingKey}) with ID {EventId}", eventType, routingKey, eventId);
        return Task.FromResult(eventId);
    }

    /// <summary>
    /// Disposes RabbitMQ connection and channel
    /// </summary>
    /// <remarks>
    /// Properly closes the channel and connection to free resources.
    /// This is typically called by the DI container at application shutdown.
    /// </remarks>
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
