namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for employee detail views
/// </summary>
/// <remarks>
/// This DTO is used when returning detailed information about a single employee
/// (e.g., employee detail page). It contains all employee information including:
/// - Complete employee profile (all fields)
/// - Department and Designation details
/// - Project memberships with project summaries
/// - Timestamps and metadata
/// 
/// This DTO is more comprehensive than EmployeeListDto and is used for:
/// - Employee detail pages
/// - Employee edit forms
/// - Full employee profiles
/// 
/// The DTO includes nested DTOs (DepartmentDto, DesignationDto, ProjectMemberDto)
/// to avoid circular references while still providing related entity information.
/// </remarks>
public class EmployeeDetailDto
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
    
    /// <summary>
    /// Indicates whether the employee is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Foreign key to Department entity
    /// </summary>
    public Guid DepartmentId { get; set; }
    
    /// <summary>
    /// Foreign key to Designation entity
    /// </summary>
    public Guid DesignationId { get; set; }
    
    /// <summary>
    /// Employee's salary
    /// </summary>
    public decimal Salary { get; set; }
    
    /// <summary>
    /// Date when the employee joined
    /// </summary>
    public DateTime JoiningDate { get; set; }
    
    /// <summary>
    /// Employee's phone number (optional)
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Employee's street address (optional)
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Employee's city (optional)
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Employee's country (optional)
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// List of employee's skills
    /// </summary>
    public List<string> Skills { get; set; } = new();
    
    /// <summary>
    /// URL to employee's avatar/profile picture (optional)
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Timestamp when the employee was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the employee was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    // Navigation properties (without circular references)
    // These are DTOs, not entities, to prevent circular reference issues
    
    /// <summary>
    /// Employee's department details (optional)
    /// </summary>
    public DepartmentDto? Department { get; set; }
    
    /// <summary>
    /// Employee's designation details (optional)
    /// </summary>
    public DesignationDto? Designation { get; set; }
    
    /// <summary>
    /// List of project memberships for this employee
    /// </summary>
    /// <remarks>
    /// Each ProjectMemberDto includes the project summary, role, and join date.
    /// This allows displaying all projects the employee is assigned to.
    /// </remarks>
    public List<ProjectMemberDto> ProjectMembers { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for department information
/// </summary>
/// <remarks>
/// Used in employee DTOs to include department information without circular references.
/// Contains only essential department fields.
/// </remarks>
public class DepartmentDto
{
    /// <summary>
    /// Department unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Department name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Department description (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Data Transfer Object for designation information
/// </summary>
/// <remarks>
/// Used in employee DTOs to include designation information without circular references.
/// Contains only essential designation fields.
/// </remarks>
public class DesignationDto
{
    /// <summary>
    /// Designation unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Designation title (e.g., "Senior Developer", "Manager")
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Designation level (optional, for hierarchy)
    /// </summary>
    public int? Level { get; set; }
    
    /// <summary>
    /// Designation description (optional)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Data Transfer Object for project membership information
/// </summary>
/// <remarks>
/// Represents the many-to-many relationship between Employees and Projects.
/// Used in EmployeeDetailDto to show all projects an employee is assigned to.
/// Includes the role and join date for each project membership.
/// </remarks>
public class ProjectMemberDto
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
    /// Role of the employee in the project (e.g., "Developer", "Lead", "QA")
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Date when the employee joined the project
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// Project summary information (optional)
    /// </summary>
    /// <remarks>
    /// Contains essential project information without circular references.
    /// ProjectSummaryDto does not include ProjectMembers to break the cycle.
    /// </remarks>
    public ProjectSummaryDto? Project { get; set; }
}

/// <summary>
/// Data Transfer Object for project summary information
/// </summary>
/// <remarks>
/// Used in ProjectMemberDto to include project information without circular references.
/// Contains only essential project fields needed for display.
/// 
/// Note: This DTO does NOT include ProjectMembers to prevent circular references.
/// If you need project members, use ProjectDetailDto instead.
/// </remarks>
public class ProjectSummaryDto
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
    
    // No ProjectMembers to break the cycle
    // If you need project members, use ProjectDetailDto instead
}
