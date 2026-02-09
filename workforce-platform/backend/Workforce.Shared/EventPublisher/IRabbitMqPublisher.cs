using Workforce.Shared.Events;

namespace Workforce.Shared.EventPublisher;

/// <summary>
/// Interface for publishing events to RabbitMQ message broker
/// Used for event-driven architecture to notify workers of domain events
/// </summary>
/// <remarks>
/// This interface abstracts RabbitMQ publishing to enable:
/// 1. Easy testing with mock implementations
/// 2. Potential future support for other message brokers
/// 3. Centralized event publishing logic
/// 
/// Events are published to a topic exchange with routing keys based on event type.
/// The eventId is returned to allow correlation with Redis-stored snapshots.
/// </remarks>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publishes an audit event to RabbitMQ
    /// </summary>
    /// <param name="eventType">The type of audit event (from AuditEventType enum)</param>
    /// <param name="eventData">The event payload data (will be JSON serialized)</param>
    /// <param name="eventId">Optional event ID. If not provided, a new GUID is generated</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>The event ID (either provided or newly generated)</returns>
    /// <exception cref="InvalidOperationException">Thrown if RabbitMQ connection is not available</exception>
    /// <remarks>
    /// The eventId is used to correlate the event with before/after snapshots stored in Redis.
    /// The routing key is automatically derived from the eventType enum using ToRoutingKey() extension.
    /// 
    /// Event publishing should occur AFTER snapshots are stored in Redis to ensure data consistency.
    /// </remarks>
    Task<string> PublishEventAsync(AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default);
}
