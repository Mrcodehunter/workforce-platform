using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

/// <summary>
/// Service implementation for audit log query operations
/// </summary>
/// <remarks>
/// Provides read-only access to audit logs stored in MongoDB.
/// This service delegates all operations to the repository.
/// </remarks>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    /// <summary>
    /// Initializes a new instance of AuditLogService
    /// </summary>
    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
    {
        return await _repository.GetRecentAsync(limit);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        return await _repository.GetByEntityTypeAsync(entityType);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId)
    {
        return await _repository.GetByEntityIdAsync(entityType, entityId);
    }

    public async Task<IEnumerable<AuditLog>> GetByEventTypeAsync(string eventType)
    {
        return await _repository.GetByEventTypeAsync(eventType);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _repository.GetByDateRangeAsync(startDate, endDate);
    }
}
