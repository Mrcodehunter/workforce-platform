namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for project detail views
/// </summary>
/// <remarks>
/// This DTO is used when returning detailed information about a single project
/// (e.g., project detail page). It contains all project information including:
/// - Complete project details (all fields)
/// - Full list of project members with employee details
/// - Full list of tasks with assignee details
/// - Timestamps and metadata
/// 
/// This DTO is more comprehensive than ProjectListDto and is used for:
/// - Project detail pages
/// - Project edit forms
/// - Full project information displays
/// 
/// The DTO includes nested DTOs (ProjectMemberWithEmployeeDto, TaskItemDto) to avoid
/// circular references while still providing related entity information.
/// </remarks>
public class ProjectDetailDto
{
    /// <summary>
    /// Project unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project status (e.g., "Active", "Completed", "OnHold")
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Project start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Project end date (optional, null for ongoing projects)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Timestamp when the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the project was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// List of project members with employee details
    /// </summary>
    /// <remarks>
    /// Each ProjectMemberWithEmployeeDto includes the membership details (role, join date)
    /// plus the employee's basic information (name, email).
    /// </remarks>
    public List<ProjectMemberWithEmployeeDto> ProjectMembers { get; set; } = new();
    
    /// <summary>
    /// List of tasks in this project
    /// </summary>
    /// <remarks>
    /// Each TaskItemDto includes task details plus the assigned employee's basic information.
    /// </remarks>
    public List<TaskItemDto> Tasks { get; set; } = new();
}

/// <summary>
/// Extended ProjectMemberDto with Employee details for ProjectDetail views
/// </summary>
/// <remarks>
/// This DTO extends the basic ProjectMemberDto by including employee information.
/// Used in ProjectDetailDto to show project members with their details.
/// </remarks>
public class ProjectMemberWithEmployeeDto
{
    /// <summary>
    /// Project unique identifier
    /// </summary>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Employee unique identifier
    /// </summary>
    public Guid EmployeeId { get; set; }
    
    /// <summary>
    /// Employee's role in the project (e.g., "Developer", "Lead", "QA")
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Date when the employee joined the project
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// Employee basic information (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential employee information (Id, FirstName, LastName, Email).
    /// Null if employee doesn't exist.
    /// </remarks>
    public EmployeeBasicDto? Employee { get; set; }
}

/// <summary>
/// Basic employee information DTO
/// </summary>
/// <remarks>
/// Used in various DTOs to include minimal employee information without circular references.
/// Contains only the most essential employee fields needed for display purposes.
/// </remarks>
public class EmployeeBasicDto
{
    /// <summary>
    /// Employee unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Employee's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Task item DTO used within ProjectDetailDto
/// </summary>
/// <remarks>
/// This DTO represents a task within a project detail view.
/// It includes task details plus the assigned employee's basic information.
/// Used in ProjectDetailDto to show all tasks for a project.
/// </remarks>
public class TaskItemDto
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
    /// Foreign key to the assigned employee (optional, nullable)
    /// </summary>
    public Guid? AssignedToEmployeeId { get; set; }
    
    /// <summary>
    /// Task priority (0=Low, 1=Medium, 2=High, 3=Critical)
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Task due date (optional)
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Assigned employee basic information (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential employee information (Id, FirstName, LastName, Email).
    /// Null if task is unassigned or employee doesn't exist.
    /// </remarks>
    public EmployeeBasicDto? AssignedToEmployee { get; set; }
}
