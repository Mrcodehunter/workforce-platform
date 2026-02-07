using MongoDB.Driver;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<LeaveRequest>? _collection;

    public LeaveRequestRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<LeaveRequest> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<LeaveRequest>("LeaveRequests");
            }
            return _collection;
        }
    }

    public async Task<object?> GetByIdAsync(string id)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}
