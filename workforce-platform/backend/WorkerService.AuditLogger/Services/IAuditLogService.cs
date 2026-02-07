namespace WorkerService.AuditLogger.Services;

public interface IAuditLogService
{
    Task LogEventAsync(string eventId, string eventType, object eventData, CancellationToken cancellationToken = default);
}
