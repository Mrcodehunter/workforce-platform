using MongoDB.Driver;
using Serilog;

namespace WorkerService.AuditLogger.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly Serilog.ILogger _logger;
    private IMongoCollection<Models.AuditLogEntry>? _auditCollection;

    public AuditLogService(IMongoDatabase mongoDatabase, Serilog.ILogger logger)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    private IMongoCollection<Models.AuditLogEntry> AuditCollection
    {
        get
        {
            if (_auditCollection == null)
            {
                _auditCollection = _mongoDatabase.GetCollection<Models.AuditLogEntry>("AuditLogs");
            }
            return _auditCollection;
        }
    }

    public async Task LogEventAsync(string eventId, string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if event already processed (idempotency)
            var existing = await AuditCollection
                .Find(a => a.EventId == eventId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null)
            {
                _logger.Information("Event {EventId} already processed, skipping", eventId);
                return;
            }

            var auditLog = new Models.AuditLogEntry
            {
                EventId = eventId,
                EventType = eventType,
                EventData = eventData,
                Timestamp = DateTime.UtcNow
            };

            await AuditCollection.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
            _logger.Information("Audit log created for event {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating audit log for event {EventId}", eventId);
            throw;
        }
    }
}
