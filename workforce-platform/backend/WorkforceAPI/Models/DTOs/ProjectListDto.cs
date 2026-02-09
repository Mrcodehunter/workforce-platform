namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for project list views
/// </summary>
/// <remarks>
/// This DTO is used when returning lists of projects (e.g., project list page).
/// It contains essential project information plus aggregated counts:
/// - Basic project information (name, description, status, dates)
/// - Member count (number of employees assigned to the project)
/// - Task count (number of tasks in the project)
/// 
/// This DTO excludes:
/// - Detailed member and task lists - use ProjectDetailDto for full details
/// - Timestamps and metadata - not needed for list views
/// 
/// The aggregated counts (MemberCount, TaskCount) are calculated by the service layer
/// to avoid loading full collections for list views, improving performance.
/// </remarks>
public class ProjectListDto
{
    /// <summary>
    /// Project unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project status (e.g., "Active", "Completed", "OnHold")
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Project start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Project end date (optional, null for ongoing projects)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Number of employees assigned to this project
    /// </summary>
    /// <remarks>
    /// This is an aggregated count calculated by the service layer.
    /// It represents the number of ProjectMember records for this project.
    /// </remarks>
    public int MemberCount { get; set; }
    
    /// <summary>
    /// Number of tasks in this project
    /// </summary>
    /// <remarks>
    /// This is an aggregated count calculated by the service layer.
    /// It represents the number of TaskItem records for this project.
    /// </remarks>
    public int TaskCount { get; set; }
}
