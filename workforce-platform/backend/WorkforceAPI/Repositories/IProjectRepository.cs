using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for project data access operations
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Gets all projects
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<Project>> GetAllAsync();
    
    /// <summary>
    /// Gets project by ID
    /// </summary>
    System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new project
    /// </summary>
    System.Threading.Tasks.Task<Project> CreateAsync(Project project);
    
    /// <summary>
    /// Updates an existing project
    /// </summary>
    System.Threading.Tasks.Task<Project> UpdateAsync(Project project);
    
    /// <summary>
    /// Reloads project with navigation properties
    /// </summary>
    System.Threading.Tasks.Task<Project?> ReloadWithNavigationPropertiesAsync(Guid id);
    
    /// <summary>
    /// Soft deletes a project
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if employee is a member of project
    /// </summary>
    System.Threading.Tasks.Task<bool> IsMemberAsync(Guid projectId, Guid employeeId);
    
    /// <summary>
    /// Adds a member to a project
    /// </summary>
    System.Threading.Tasks.Task<ProjectMember> AddMemberAsync(ProjectMember member);
    
    /// <summary>
    /// Removes a member from a project
    /// </summary>
    System.Threading.Tasks.Task RemoveMemberAsync(Guid projectId, Guid employeeId);
}
