namespace WorkforceAPI.Models;

public class Designation
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Level { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
