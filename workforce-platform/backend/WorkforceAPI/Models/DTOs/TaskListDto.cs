namespace WorkforceAPI.Models.DTOs;

public class TaskListDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToEmployeeId { get; set; }
    public ProjectSummaryDto? Project { get; set; }
    public EmployeeBasicDto? AssignedToEmployee { get; set; }
}
