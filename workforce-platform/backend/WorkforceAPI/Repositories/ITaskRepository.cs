using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for task data access operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets all tasks
    /// </summary>
    Task<IEnumerable<TaskItem>> GetAllAsync();
    
    /// <summary>
    /// Gets task by ID
    /// </summary>
    Task<TaskItem?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets tasks by project ID
    /// </summary>
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId);
    
    /// <summary>
    /// Gets tasks by employee ID
    /// </summary>
    Task<IEnumerable<TaskItem>> GetByEmployeeIdAsync(Guid employeeId);
    
    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<TaskItem> CreateAsync(TaskItem task);
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TaskItem> UpdateAsync(TaskItem task);
    
    /// <summary>
    /// Soft deletes a task
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
