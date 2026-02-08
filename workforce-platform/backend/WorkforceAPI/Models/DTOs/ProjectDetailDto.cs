namespace WorkforceAPI.Models.DTOs;

public class ProjectDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public List<ProjectMemberWithEmployeeDto> ProjectMembers { get; set; } = new();
    public List<TaskItemDto> Tasks { get; set; } = new();
}

// Extended ProjectMemberDto with Employee details for ProjectDetail
public class ProjectMemberWithEmployeeDto
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public EmployeeBasicDto? Employee { get; set; }
}

public class EmployeeBasicDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// TaskItemDto is used within ProjectDetailDto for tasks
public class TaskItemDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedToEmployeeId { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public EmployeeBasicDto? AssignedToEmployee { get; set; }
}
