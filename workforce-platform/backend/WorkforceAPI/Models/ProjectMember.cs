namespace WorkforceAPI.Models;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
