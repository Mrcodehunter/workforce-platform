namespace WorkforceAPI.Models;

/// <summary>
/// TaskItem entity representing a task within a project
/// </summary>
/// <remarks>
/// This entity represents a task that belongs to a project and can be assigned to an employee.
/// Tasks track work items with status, priority, and due dates.
/// 
/// The entity uses:
/// - Soft delete pattern (IsDeleted flag)
/// - Navigation properties for related entities (Project, AssignedToEmployee)
/// - Foreign key to Project (required)
/// - Optional foreign key to Employee (nullable - tasks can be unassigned)
/// - Status tracking (ToDo, InProgress, InReview, Done, Cancelled)
/// - Priority levels (0=Low, 1=Medium, 2=High, 3=Critical)
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// Validation rules are defined in TaskValidator (FluentValidation).
/// </remarks>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Project entity (required)
    /// </summary>
    /// <remarks>
    /// Every task must belong to a project.
    /// Delete behavior: Cascade (when project is deleted, tasks are removed).
    /// </remarks>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Task title (required, max 200 characters)
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description (optional, max 1000 characters)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Task status (default: "ToDo")
    /// </summary>
    /// <remarks>
    /// Valid values: "ToDo", "InProgress", "InReview", "Done", "Cancelled"
    /// </remarks>
    public string Status { get; set; } = "ToDo"; // ToDo, InProgress, InReview, Done, Cancelled
    
    /// <summary>
    /// Foreign key to the Employee entity (optional, nullable)
    /// </summary>
    /// <remarks>
    /// Tasks can be unassigned (null) or assigned to an employee.
    /// Delete behavior: SetNull (when employee is deleted, AssignedToEmployeeId is set to null).
    /// </remarks>
    public Guid? AssignedToEmployeeId { get; set; }
    
    /// <summary>
    /// Task priority level (default: 0 = Low)
    /// </summary>
    /// <remarks>
    /// Valid values: 0=Low, 1=Medium, 2=High, 3=Critical
    /// Must be between 0 and 3 (validation rule).
    /// </remarks>
    public int Priority { get; set; } = 0; // 0=Low, 1=Medium, 2=High, 3=Critical
    
    /// <summary>
    /// Task due date (optional)
    /// </summary>
    /// <remarks>
    /// If provided, must be in the future (validation rule).
    /// </remarks>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Timestamp when the task was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the task was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Soft delete flag - indicates if the task is deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    // These are loaded by Entity Framework Core when explicitly included in queries
    
    /// <summary>
    /// Navigation property to the project this task belongs to
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Null if ProjectId doesn't match an existing project.
    /// </remarks>
    public Project? Project { get; set; }
    
    /// <summary>
    /// Navigation property to the employee assigned to this task
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Null if task is unassigned or AssignedToEmployeeId doesn't match an existing employee.
    /// </remarks>
    public Employee? AssignedToEmployee { get; set; }
}
