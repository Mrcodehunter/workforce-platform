namespace WorkforceAPI.Models;

/// <summary>
/// Employee entity representing a workforce member
/// </summary>
/// <remarks>
/// This is the primary entity in the workforce management system.
/// It represents an employee with all their personal, professional, and organizational information.
/// 
/// The entity uses:
/// - Soft delete pattern (IsDeleted flag) instead of physical deletion
/// - Navigation properties for related entities (Department, Designation, Projects, Tasks)
/// - Foreign key IDs for relationships (DepartmentId, DesignationId)
/// - Timestamps for audit tracking (CreatedAt, UpdatedAt)
/// 
/// Entity Framework Core configuration is defined in WorkforceDbContext.OnModelCreating().
/// Validation rules are defined in EmployeeValidator (FluentValidation).
/// </remarks>
public class Employee
{
    /// <summary>
    /// Unique identifier for the employee (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Employee's first name (required, max 100 characters)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee's last name (required, max 100 characters)
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee's email address (required, unique, max 255 characters)
    /// </summary>
    /// <remarks>
    /// Email must be unique across all employees (enforced by database index).
    /// Used for authentication and communication.
    /// </remarks>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether the employee is currently active
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Inactive employees are typically on leave or terminated but not deleted.
    /// </remarks>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Foreign key to the Department entity
    /// </summary>
    /// <remarks>
    /// Required. Cannot be empty GUID.
    /// Delete behavior: Restrict (cannot delete department if employees are assigned).
    /// </remarks>
    public Guid DepartmentId { get; set; }
    
    /// <summary>
    /// Foreign key to the Designation entity
    /// </summary>
    /// <remarks>
    /// Required. Cannot be empty GUID.
    /// Delete behavior: Restrict (cannot delete designation if employees have it).
    /// </remarks>
    public Guid DesignationId { get; set; }
    
    /// <summary>
    /// Employee's salary (precision: 18,2 - max 999,999,999,999,999,999.99)
    /// </summary>
    public decimal Salary { get; set; }
    
    /// <summary>
    /// Date when the employee joined the organization
    /// </summary>
    /// <remarks>
    /// Required. Cannot be in the future (business rule).
    /// </remarks>
    public DateTime JoiningDate { get; set; }
    
    /// <summary>
    /// Employee's phone number (optional, max 20 characters)
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Employee's street address (optional, max 500 characters)
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Employee's city (optional, max 100 characters)
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Employee's country (optional, max 100 characters)
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// List of employee's skills (stored as PostgreSQL JSONB array)
    /// </summary>
    /// <remarks>
    /// Each skill is a string (max 50 characters per skill).
    /// Stored as JSONB in PostgreSQL for efficient querying.
    /// </remarks>
    public List<string> Skills { get; set; } = new();
    
    /// <summary>
    /// URL to employee's avatar/profile picture (optional, max 500 characters, must be valid URL)
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Timestamp when the employee record was created (UTC)
    /// </summary>
    /// <remarks>
    /// Automatically set when the employee is created.
    /// Never updated after creation.
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the employee record was last updated (UTC)
    /// </summary>
    /// <remarks>
    /// Automatically updated whenever the employee is modified.
    /// Initially set to CreatedAt value.
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Soft delete flag - indicates if the employee is deleted
    /// </summary>
    /// <remarks>
    /// Default: false
    /// When true, the employee is considered deleted but the record remains in the database.
    /// This allows for data recovery and maintains referential integrity.
    /// </remarks>
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    // These are loaded by Entity Framework Core when explicitly included in queries
    
    /// <summary>
    /// Navigation property to the employee's department
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Null if DepartmentId doesn't match an existing department.
    /// </remarks>
    public Department? Department { get; set; }
    
    /// <summary>
    /// Navigation property to the employee's designation
    /// </summary>
    /// <remarks>
    /// Loaded via Include() in LINQ queries.
    /// Null if DesignationId doesn't match an existing designation.
    /// </remarks>
    public Designation? Designation { get; set; }
    
    /// <summary>
    /// Collection of project memberships for this employee
    /// </summary>
    /// <remarks>
    /// Represents the many-to-many relationship between Employees and Projects.
    /// Each ProjectMember contains the role and join date for a project.
    /// Delete behavior: Cascade (when employee is deleted, memberships are removed).
    /// </remarks>
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    /// <summary>
    /// Collection of tasks assigned to this employee
    /// </summary>
    /// <remarks>
    /// Represents tasks where this employee is the assignee.
    /// Delete behavior: SetNull (when employee is deleted, AssignedToEmployeeId is set to null).
    /// </remarks>
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
