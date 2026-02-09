namespace WorkforceAPI.Models.DTOs;

public class ProjectListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
}
