namespace WorkforceAPI.Models;

/// <summary>
/// ProjectMember entity representing the many-to-many relationship between Projects and Employees
/// </summary>
/// <remarks>
/// This entity represents the join table for the many-to-many relationship between Projects and Employees.
/// It stores additional information about the relationship:
/// - Role: The employee's role in the project (e.g., "Developer", "Lead", "QA", "Manager")
/// - JoinedAt: When the employee joined the project
/// 
/// The entity uses:
/// - Composite primary key (ProjectId, EmployeeId) - ensures an employee can only be a member once
/// - Navigation properties to both Project and Employee
/// - Cascade delete on both sides (when project or employee is deleted, memberships are removed)
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// This is a pure join table with no soft delete (memberships are removed, not soft-deleted).
/// </remarks>
public class ProjectMember
{
    /// <summary>
    /// Foreign key to the Project entity (part of composite primary key)
    /// </summary>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Foreign key to the Employee entity (part of composite primary key)
    /// </summary>
    public Guid EmployeeId { get; set; }
    
    /// <summary>
    /// Employee's role in the project (optional)
    /// </summary>
    /// <remarks>
    /// Examples: "Developer", "Lead Developer", "QA Engineer", "Project Manager", "Tech Lead", "Architect"
    /// </remarks>
    public string? Role { get; set; } // Developer, Lead, QA, Manager, etc.
    
    /// <summary>
    /// Date when the employee joined the project (default: current UTC time)
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    // These are loaded by Entity Framework Core when explicitly included in queries
    
    /// <summary>
    /// Navigation property to the project
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Delete behavior: Cascade (when project is deleted, memberships are removed).
    /// </remarks>
    public Project? Project { get; set; }
    
    /// <summary>
    /// Navigation property to the employee
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Delete behavior: Cascade (when employee is deleted, memberships are removed).
    /// </remarks>
    public Employee? Employee { get; set; }
}
