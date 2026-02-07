using MongoDB.Driver;

namespace WorkforceAPI.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<object>? _collection;

    public LeaveRequestRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<object> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<object>("LeaveRequests");
            }
            return _collection;
        }
    }

    public async Task<object?> GetByIdAsync(string id)
    {
        var filter = Builders<object>.Filter.Eq("_id", id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}
