using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for project-related business operations
/// </summary>
/// <remarks>
/// This interface defines the contract for project management operations including
/// CRUD operations and project member management.
/// </remarks>
public interface IProjectService
{
    /// <summary>
    /// Retrieves all projects as a list
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<ProjectListDto>> GetAllAsync();
    
    /// <summary>
    /// Retrieves a single project by ID with full details
    /// </summary>
    System.Threading.Tasks.Task<ProjectDetailDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new project
    /// </summary>
    System.Threading.Tasks.Task<Project> CreateAsync(Project project);
    
    /// <summary>
    /// Updates an existing project
    /// </summary>
    System.Threading.Tasks.Task<Project> UpdateAsync(Project project);
    
    /// <summary>
    /// Deletes a project (soft delete)
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Adds an employee as a member to a project
    /// </summary>
    Task<ProjectDetailDto> AddMemberAsync(Guid projectId, Guid employeeId, string? role);
    
    /// <summary>
    /// Removes an employee from a project
    /// </summary>
    Task<ProjectDetailDto> RemoveMemberAsync(Guid projectId, Guid employeeId);
}
