using MongoDB.Driver;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository implementation for audit log data access operations (MongoDB)
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<AuditLog>? _collection;

    /// <summary>
    /// Initializes a new instance of AuditLogRepository
    /// </summary>
    public AuditLogRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<AuditLog> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<AuditLog>("AuditLogs");
            }
            return _collection;
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await Collection.Find(_ => true)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
    {
        return await Collection.Find(_ => true)
            .SortByDescending(x => x.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType);
        return await Collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId)
    {
        var filter = Builders<AuditLog>.Filter.And(
            Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType),
            Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId)
        );
        return await Collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEventTypeAsync(string eventType)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.EventType, eventType);
        return await Collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<AuditLog>.Filter.And(
            Builders<AuditLog>.Filter.Gte(x => x.Timestamp, startDate),
            Builders<AuditLog>.Filter.Lte(x => x.Timestamp, endDate)
        );
        return await Collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }
}
