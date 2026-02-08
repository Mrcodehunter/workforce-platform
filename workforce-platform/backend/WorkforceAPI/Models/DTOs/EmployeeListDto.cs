namespace WorkforceAPI.Models.DTOs;

public class EmployeeListDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DepartmentDto? Department { get; set; }
    public DesignationDto? Designation { get; set; }
}
