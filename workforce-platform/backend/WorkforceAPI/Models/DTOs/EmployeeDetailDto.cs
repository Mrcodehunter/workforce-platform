namespace WorkforceAPI.Models.DTOs;

public class EmployeeDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DesignationId { get; set; }
    public decimal Salary { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public List<string> Skills { get; set; } = new();
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties (without circular references)
    public DepartmentDto? Department { get; set; }
    public DesignationDto? Designation { get; set; }
    public List<ProjectMemberDto> ProjectMembers { get; set; } = new();
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DesignationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Level { get; set; }
    public string? Description { get; set; }
}

public class ProjectMemberDto
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public ProjectSummaryDto? Project { get; set; }
}

public class ProjectSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    // No ProjectMembers to break the cycle
}
