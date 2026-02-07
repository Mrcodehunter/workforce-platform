namespace WorkerService.AuditLogger.Services;

public interface IRabbitMqConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
