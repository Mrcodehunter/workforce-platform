using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

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
