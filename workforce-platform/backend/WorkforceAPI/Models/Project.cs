namespace WorkforceAPI.Models;

/// <summary>
/// Project entity representing a work project
/// </summary>
/// <remarks>
/// This entity represents a project in the workforce management system.
/// Projects can have multiple members (employees) and multiple tasks.
/// 
/// The entity uses:
/// - Soft delete pattern (IsDeleted flag)
/// - Navigation properties for related entities (ProjectMembers, Tasks)
/// - Status tracking (Planning, Active, OnHold, Completed, Cancelled)
/// - Date range tracking (StartDate, optional EndDate)
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// Validation rules are defined in ProjectValidator (FluentValidation).
/// </remarks>
public class Project
{
    /// <summary>
    /// Unique identifier for the project (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Project name (required, max 200 characters)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description (optional, max 1000 characters)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project status (default: "Planning")
    /// </summary>
    /// <remarks>
    /// Valid values: "Planning", "Active", "OnHold", "Completed", "Cancelled"
    /// </remarks>
    public string Status { get; set; } = "Planning"; // Planning, Active, OnHold, Completed, Cancelled
    
    /// <summary>
    /// Project start date (required)
    /// </summary>
    /// <remarks>
    /// Cannot be more than 1 year in the future (validation rule).
    /// </remarks>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Project end date (optional, null for ongoing projects)
    /// </summary>
    /// <remarks>
    /// If provided, must be after StartDate (validation rule).
    /// </remarks>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Timestamp when the project was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the project was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Soft delete flag - indicates if the project is deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    // These are loaded by Entity Framework Core when explicitly included in queries
    
    /// <summary>
    /// Collection of project members (employees assigned to this project)
    /// </summary>
    /// <remarks>
    /// Represents the many-to-many relationship between Projects and Employees.
    /// Each ProjectMember contains the role and join date for an employee.
    /// Delete behavior: Cascade (when project is deleted, memberships are removed).
    /// </remarks>
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    /// <summary>
    /// Collection of tasks associated with this project
    /// </summary>
    /// <remarks>
    /// All tasks that belong to this project.
    /// Delete behavior: Cascade (when project is deleted, tasks are removed).
    /// </remarks>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
