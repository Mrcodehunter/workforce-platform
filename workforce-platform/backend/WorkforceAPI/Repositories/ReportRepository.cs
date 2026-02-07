using MongoDB.Driver;

namespace WorkforceAPI.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<object>? _collection;

    public ReportRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<object> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<object>("Reports");
            }
            return _collection;
        }
    }

    public async Task<object?> GetLatestAsync()
    {
        var sort = Builders<object>.Sort.Descending("generatedAt");
        return await Collection.Find(_ => true).Sort(sort).FirstOrDefaultAsync();
    }
}
