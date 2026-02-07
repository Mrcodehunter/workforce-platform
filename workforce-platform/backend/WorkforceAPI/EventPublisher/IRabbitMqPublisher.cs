namespace WorkforceAPI.EventPublisher;

public interface IRabbitMqPublisher
{
    Task PublishEventAsync(string eventType, object eventData, CancellationToken cancellationToken = default);
}
