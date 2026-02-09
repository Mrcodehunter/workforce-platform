namespace WorkforceAPI.Models;

/// <summary>
/// Department entity representing an organizational department
/// </summary>
/// <remarks>
/// This entity represents a department within the organization (e.g., Engineering, Sales, HR).
/// Departments are assigned to employees to organize the workforce.
/// 
/// The entity uses:
/// - Soft delete pattern (IsDeleted flag)
/// - Unique name constraint (enforced by database index)
/// - Timestamps for audit tracking
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// The Name field has a unique index to prevent duplicate department names.
/// </remarks>
public class Department
{
    /// <summary>
    /// Unique identifier for the department (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Department name (required, unique, max length depends on database)
    /// </summary>
    /// <remarks>
    /// Must be unique across all departments (enforced by database unique index).
    /// </remarks>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Department description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Timestamp when the department was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the department was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Soft delete flag - indicates if the department is deleted
    /// </summary>
    /// <remarks>
    /// Delete behavior: Restrict (cannot delete department if employees are assigned to it).
    /// </remarks>
    public bool IsDeleted { get; set; } = false;
}
