using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLog>> GetByEventTypeAsync(string eventType);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
