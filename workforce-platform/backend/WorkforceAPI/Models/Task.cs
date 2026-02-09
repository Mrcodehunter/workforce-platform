namespace WorkforceAPI.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "ToDo"; // ToDo, InProgress, InReview, Done, Cancelled
    public Guid? AssignedToEmployeeId { get; set; }
    public int Priority { get; set; } = 0; // 0=Low, 1=Medium, 2=High, 3=Critical
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Project? Project { get; set; }
    public Employee? AssignedToEmployee { get; set; }
}
