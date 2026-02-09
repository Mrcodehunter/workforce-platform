using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for audit log query operations
/// </summary>
/// <remarks>
/// Provides methods to query audit logs stored in MongoDB with various filters.
/// </remarks>
public interface IAuditLogService
{
    /// <summary>
    /// Retrieves all audit logs
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAllAsync();
    
    /// <summary>
    /// Retrieves recent audit logs with limit
    /// </summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
    
    /// <summary>
    /// Retrieves audit logs by entity type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    
    /// <summary>
    /// Retrieves audit logs by entity type and ID
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(string entityType, string entityId);
    
    /// <summary>
    /// Retrieves audit logs by event type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEventTypeAsync(string eventType);
    
    /// <summary>
    /// Retrieves audit logs within a date range
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
