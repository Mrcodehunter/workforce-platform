using System.ComponentModel.DataAnnotations;

namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Request DTO for adding a project member
/// </summary>
public class AddProjectMemberRequestDto
{
    [Required]
    public Guid EmployeeId { get; set; }
    
    [MaxLength(100)]
    public string? Role { get; set; }
}

/// <summary>
/// Response DTO for project member operations
/// </summary>
public class ProjectMemberResponseDto
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public EmployeeBasicDto? Employee { get; set; }
}
