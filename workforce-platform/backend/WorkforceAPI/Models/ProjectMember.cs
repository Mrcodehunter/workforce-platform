namespace WorkforceAPI.Models;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; } // Developer, Lead, QA, Manager, etc.
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project? Project { get; set; }
    public Employee? Employee { get; set; }
}
