using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for audit log data access operations (MongoDB)
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets all audit logs
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAllAsync();
    
    /// <summary>
    /// Gets recent audit logs
    /// </summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
    
    /// <summary>
    /// Gets audit logs by entity type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    
    /// <summary>
    /// Gets audit logs by entity type and ID
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets audit logs by event type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEventTypeAsync(string eventType);
    
    /// <summary>
    /// Gets audit logs by date range
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
