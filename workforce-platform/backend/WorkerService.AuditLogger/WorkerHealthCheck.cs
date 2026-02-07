using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace WorkerService.AuditLogger;

public class WorkerHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _mongoDatabase;

    public WorkerHealthCheck(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check MongoDB connection
            await _mongoDatabase.RunCommandAsync<object>(
                new BsonDocumentCommand<object>(new BsonDocument("ping", 1)),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("MongoDB connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB connection failed", ex);
        }
    }
}
