using Workforce.Shared.Events;

namespace Workforce.Shared.EventPublisher;

public interface IRabbitMqPublisher
{
    Task<string> PublishEventAsync(AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default);
}
