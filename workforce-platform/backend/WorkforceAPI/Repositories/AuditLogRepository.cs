using MongoDB.Driver;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<AuditLog>? _collection;

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

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}
