using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WorkforceAPI.Data;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

/// <summary>
/// Service implementation for dashboard summary operations
/// </summary>
/// <remarks>
/// Aggregates data from both PostgreSQL (employees, projects, tasks) and MongoDB (leave requests)
/// to provide comprehensive dashboard statistics. This service directly accesses DbContext
/// and MongoDB for performance reasons (aggregation queries).
/// </remarks>
public class DashboardService : IDashboardService
{
    private readonly WorkforceDbContext _dbContext;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DashboardService> _logger;

    /// <summary>
    /// Initializes a new instance of DashboardService
    /// </summary>
    public DashboardService(
        WorkforceDbContext dbContext,
        IMongoDatabase mongoDatabase,
        ILogger<DashboardService> logger)
    {
        _dbContext = dbContext;
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    public async Task<object> GetSummaryAsync()
    {
        try
        {
            // Get data from PostgreSQL
            var employees = await _dbContext.Employees.Where(e => !e.IsDeleted).ToListAsync();
            var projects = await _dbContext.Projects
                .Include(p => p.Tasks)
                .Include(p => p.ProjectMembers)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
            var tasks = await _dbContext.Tasks.Where(t => !t.IsDeleted).ToListAsync();
            var departments = await _dbContext.Departments.ToListAsync();

            // Get data from MongoDB
            var leaveRequestsCollection = _mongoDatabase.GetCollection<LeaveRequest>("LeaveRequests");
            var leaveRequests = await leaveRequestsCollection.Find(_ => true).ToListAsync();

            // Build summary
            var summary = new DashboardSummaryDto
            {
                TotalEmployees = employees.Count,
                ActiveEmployees = employees.Count(e => e.IsActive),
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.Status == "Active"),
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == "Done"),
                PendingLeaveRequests = leaveRequests.Count(lr => lr.Status == "Pending"),
            };

            // Department headcount
            summary.DepartmentHeadcount = departments.Select(dept => new DepartmentHeadcountDto
            {
                DepartmentId = dept.Id.ToString(),
                DepartmentName = dept.Name,
                Count = employees.Count(e => e.DepartmentId == dept.Id && !e.IsDeleted)
            }).ToList();

            // Project progress
            summary.ProjectProgress = projects.Select(project => new ProjectProgressDto
            {
                ProjectId = project.Id.ToString(),
                ProjectName = project.Name,
                Status = project.Status,
                TotalTasks = project.Tasks?.Count(t => !t.IsDeleted) ?? 0,
                CompletedTasks = project.Tasks?.Count(t => t.Status == "Done" && !t.IsDeleted) ?? 0,
                ProgressPercentage = project.Tasks != null && project.Tasks.Any(t => !t.IsDeleted)
                    ? (int)Math.Round((double)project.Tasks.Count(t => t.Status == "Done" && !t.IsDeleted) / project.Tasks.Count(t => !t.IsDeleted) * 100)
                    : 0
            }).ToList();

            // Leave statistics
            summary.LeaveStatistics = new LeaveStatisticsDto
            {
                TotalRequests = leaveRequests.Count,
                Approved = leaveRequests.Count(lr => lr.Status == "Approved"),
                Pending = leaveRequests.Count(lr => lr.Status == "Pending"),
                Rejected = leaveRequests.Count(lr => lr.Status == "Rejected"),
                ByType = leaveRequests
                    .GroupBy(lr => lr.LeaveType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary");
            // Return empty summary on error
            return new DashboardSummaryDto();
        }
    }
}
