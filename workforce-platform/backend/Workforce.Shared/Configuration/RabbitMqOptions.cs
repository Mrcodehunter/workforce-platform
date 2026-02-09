namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for RabbitMQ message broker
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ host name (default: "rabbitmq" for Docker, "localhost" for local)
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ port (default: 5672)
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username (default: "guest")
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password (default: "guest")
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Exchange name for events (default: "workforce.events")
    /// </summary>
    public string ExchangeName { get; set; } = "workforce.events";

    /// <summary>
    /// Exchange type (default: "topic")
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Queue name for audit logger (default: "audit.queue")
    /// </summary>
    public string QueueName { get; set; } = "audit.queue";

    /// <summary>
    /// Routing keys to bind (default: ["#"] for all events)
    /// </summary>
    public string[] RoutingKeys { get; set; } = new[] { "#" };

    /// <summary>
    /// Enable RabbitMQ event publishing (default: true)
    /// If false, events will not be published
    /// </summary>
    public bool Enabled { get; set; } = true;
}
