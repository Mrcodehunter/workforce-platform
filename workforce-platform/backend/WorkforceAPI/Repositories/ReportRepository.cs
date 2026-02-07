using MongoDB.Driver;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<Report>? _collection;

    public ReportRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<Report> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<Report>("Reports");
            }
            return _collection;
        }
    }

    public async Task<object?> GetLatestAsync()
    {
        var sort = Builders<Report>.Sort.Descending(x => x.GeneratedAt);
        return await Collection.Find(_ => true).Sort(sort).FirstOrDefaultAsync();
    }
}
