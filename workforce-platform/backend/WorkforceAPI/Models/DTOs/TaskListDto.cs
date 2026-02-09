namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for task list views
/// </summary>
/// <remarks>
/// This DTO is used when returning lists of tasks (e.g., task list page, task board).
/// It contains essential task information needed for list displays:
/// - Basic task information (title, description, status, priority)
/// - Project summary (not full project details)
/// - Assigned employee summary (not full employee details)
/// 
/// This DTO excludes:
/// - Timestamps (CreatedAt, UpdatedAt) - not needed for list views
/// - Detailed information - use TaskDetailDto for full details
/// 
/// The purpose is to:
/// 1. Reduce payload size (only send necessary data)
/// 2. Improve API performance (less data to serialize/transfer)
/// 3. Control what data is exposed to clients
/// 4. Avoid circular reference issues in JSON serialization
/// </remarks>
public class TaskListDto
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
    /// Foreign key to the assigned employee (optional, nullable)
    /// </summary>
    public Guid? AssignedToEmployeeId { get; set; }
    
    /// <summary>
    /// Project summary information (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential project information (Id, Name, Description, Status, Dates).
    /// Null if project doesn't exist.
    /// </remarks>
    public ProjectSummaryDto? Project { get; set; }
    
    /// <summary>
    /// Assigned employee summary information (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential employee information (Id, FirstName, LastName, Email).
    /// Null if task is unassigned or employee doesn't exist.
    /// </remarks>
    public EmployeeBasicDto? AssignedToEmployee { get; set; }
}
