namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for task detail views
/// </summary>
/// <remarks>
/// This DTO is used when returning detailed information about a single task
/// (e.g., task detail page, task edit form). It contains all task information including:
/// - Complete task details (all fields)
/// - Project summary
/// - Assigned employee summary
/// - Timestamps and metadata
/// 
/// This DTO is more comprehensive than TaskListDto and is used for:
/// - Task detail pages
/// - Task edit forms
/// - Full task information displays
/// 
/// The DTO includes nested DTOs (ProjectSummaryDto, EmployeeBasicDto) to avoid
/// circular references while still providing related entity information.
/// </remarks>
public class TaskDetailDto
{
    /// <summary>
    /// Task unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Project entity
    /// </summary>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Task status (e.g., "ToDo", "InProgress", "Done")
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Task priority (0=Low, 1=Medium, 2=High, 3=Critical)
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Task due date (optional)
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Timestamp when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the task was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Foreign key to the assigned employee (optional, nullable)
    /// </summary>
    public Guid? AssignedToEmployeeId { get; set; }
    
    /// <summary>
    /// Project summary information (optional)
    /// </summary>
    public ProjectSummaryDto? Project { get; set; }
    
    /// <summary>
    /// Assigned employee summary information (optional)
    /// </summary>
    public EmployeeBasicDto? AssignedToEmployee { get; set; }
}
