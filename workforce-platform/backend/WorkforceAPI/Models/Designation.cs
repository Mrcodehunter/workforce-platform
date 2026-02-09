namespace WorkforceAPI.Models;

/// <summary>
/// Designation entity representing a job title/position
/// </summary>
/// <remarks>
/// This entity represents a job designation or title (e.g., "Senior Developer", "Manager").
/// Designations are assigned to employees to indicate their role and level.
/// 
/// The entity uses:
/// - Unique title constraint (enforced by database index)
/// - Optional level field for hierarchy (e.g., 1=Junior, 5=Senior)
/// - Timestamps for audit tracking
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// The Title field has a unique index to prevent duplicate designation titles.
/// </remarks>
public class Designation
{
    /// <summary>
    /// Unique identifier for the designation (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Designation title (required, unique, max length depends on database)
    /// </summary>
    /// <remarks>
    /// Examples: "Junior Developer", "Senior Developer", "Engineering Manager"
    /// Must be unique across all designations (enforced by database unique index).
    /// </remarks>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Designation level (optional, for hierarchy)
    /// </summary>
    /// <remarks>
    /// Used to establish hierarchy (e.g., 1=Junior, 2=Mid, 3=Senior, 4=Lead, 5=Manager).
    /// Higher numbers typically indicate higher seniority.
    /// </remarks>
    public int? Level { get; set; }
    
    /// <summary>
    /// Designation description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Timestamp when the designation was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the designation was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
