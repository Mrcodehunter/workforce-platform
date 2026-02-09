namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for employee list views
/// </summary>
/// <remarks>
/// This DTO is used when returning lists of employees (e.g., employee list page, dropdowns).
/// It contains only the essential information needed for list displays:
/// - Basic employee information (name, email, status)
/// - Contact information (phone, avatar)
/// - Department and Designation summaries (not full entities)
/// 
/// This DTO excludes:
/// - Detailed information (salary, address, skills) - use EmployeeDetailDto for details
/// - Project memberships - use EmployeeDetailDto for full details
/// - Timestamps and metadata - not needed for list views
/// 
/// The purpose is to:
/// 1. Reduce payload size (only send necessary data)
/// 2. Improve API performance (less data to serialize/transfer)
/// 3. Control what data is exposed to clients
/// 4. Avoid circular reference issues in JSON serialization
/// </remarks>
public class EmployeeListDto
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
    /// Employee's phone number (optional)
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// URL to employee's avatar/profile picture (optional)
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Employee's department summary (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential department information (Id, Name, Description).
    /// Null if employee is not assigned to a department.
    /// </remarks>
    public DepartmentDto? Department { get; set; }
    
    /// <summary>
    /// Employee's designation summary (optional)
    /// </summary>
    /// <remarks>
    /// Contains only essential designation information (Id, Title, Level, Description).
    /// Null if employee doesn't have a designation.
    /// </remarks>
    public DesignationDto? Designation { get; set; }
}
