namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for RabbitMQ message broker connection and event publishing
/// </summary>
/// <remarks>
/// This class is bound from the "RabbitMQ" configuration section in appsettings.json.
/// It provides strongly-typed access to RabbitMQ configuration settings.
/// 
/// Environment-specific defaults:
/// - Development: "localhost" (local RabbitMQ instance)
/// - Production: "rabbitmq" (Docker service name)
/// 
/// RabbitMQ is used for event-driven architecture, publishing domain events
/// (e.g., EmployeeCreated, ProjectUpdated) to worker services.
/// </remarks>
public class RabbitMqOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ host name or IP address
    /// </summary>
    /// <remarks>
    /// Default: "localhost" (Development), "rabbitmq" (Production/Docker)
    /// 
    /// Examples:
    /// - "localhost" (local development)
    /// - "rabbitmq" (Docker Compose service name)
    /// - "rabbitmq.example.com" (production hostname)
    /// </remarks>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ AMQP port
    /// </summary>
    /// <remarks>
    /// Default: 5672 (standard AMQP port)
    /// 
    /// Note: Management UI typically runs on port 15672, but this is for AMQP connections.
    /// </remarks>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username for authentication
    /// </summary>
    /// <remarks>
    /// Default: "guest" (RabbitMQ default)
    /// 
    /// In production, use a dedicated user with appropriate permissions.
    /// The "guest" user is typically restricted to localhost connections only.
    /// </remarks>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password for authentication
    /// </summary>
    /// <remarks>
    /// Default: "guest" (RabbitMQ default)
    /// 
    /// In production, use a strong password stored in secure configuration
    /// (e.g., environment variables, Azure Key Vault, etc.).
    /// </remarks>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Name of the topic exchange for publishing events
    /// </summary>
    /// <remarks>
    /// Default: "workforce.events"
    /// 
    /// All domain events are published to this exchange. The exchange is declared
    /// as durable (survives broker restarts) and of type "topic" to support
    /// routing key pattern matching (e.g., "employee.*", "project.member.*").
    /// </remarks>
    public string ExchangeName { get; set; } = "workforce.events";

    /// <summary>
    /// Type of RabbitMQ exchange
    /// </summary>
    /// <remarks>
    /// Default: "topic"
    /// 
    /// Topic exchanges allow routing based on pattern matching:
    /// - "employee.*" matches all employee events
    /// - "project.member.*" matches all project member events
    /// - "#" matches all events
    /// 
    /// This enables flexible subscription patterns for worker services.
    /// </remarks>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Queue name for the audit logger worker
    /// </summary>
    /// <remarks>
    /// Default: "audit.queue"
    /// 
    /// This is used by the WorkerService.AuditLogger to bind its queue
    /// to the exchange. The queue is declared as durable to survive broker restarts.
    /// 
    /// Note: This setting is primarily for the worker service, not the API.
    /// </remarks>
    public string QueueName { get; set; } = "audit.queue";

    /// <summary>
    /// Routing key patterns to bind to the queue
    /// </summary>
    /// <remarks>
    /// Default: ["#"] (all events)
    /// 
    /// The "#" routing key matches all events. More specific patterns can be used:
    /// - ["employee.*"] - only employee events
    /// - ["project.*", "task.*"] - project and task events
    /// 
    /// This setting is primarily for the worker service configuration.
    /// </remarks>
    public string[] RoutingKeys { get; set; } = new[] { "#" };

    /// <summary>
    /// Enable or disable RabbitMQ event publishing
    /// </summary>
    /// <remarks>
    /// Default: true
    /// 
    /// If false:
    /// - No connection to RabbitMQ will be established
    /// - Events will not be published (no-op publisher is used)
    /// - Useful for local development without RabbitMQ, or for testing
    /// 
    /// When disabled, the application will function normally but events won't
    /// be sent to worker services (e.g., audit logging won't occur).
    /// </remarks>
    public bool Enabled { get; set; } = true;
}
