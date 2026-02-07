using MongoDB.Driver;

namespace WorkforceAPI.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<object>? _collection;

    public AuditLogRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<object> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<object>("AuditLogs");
            }
            return _collection;
        }
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}
