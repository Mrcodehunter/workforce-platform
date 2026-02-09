namespace WorkforceAPI.Models.DTOs;

public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingLeaveRequests { get; set; }
    public List<DepartmentHeadcountDto> DepartmentHeadcount { get; set; } = new();
    public List<ProjectProgressDto> ProjectProgress { get; set; } = new();
    public LeaveStatisticsDto LeaveStatistics { get; set; } = new();
    public List<object> RecentActivity { get; set; } = new();
}

public class DepartmentHeadcountDto
{
    public string DepartmentId { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProjectProgressDto
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercentage { get; set; }
}

public class LeaveStatisticsDto
{
    public int TotalRequests { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}
