using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for department data access operations
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// Gets all departments
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<Department>> GetAllAsync();
    
    /// <summary>
    /// Gets department by ID
    /// </summary>
    System.Threading.Tasks.Task<Department?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new department
    /// </summary>
    System.Threading.Tasks.Task<Department> CreateAsync(Department department);
    
    /// <summary>
    /// Updates an existing department
    /// </summary>
    System.Threading.Tasks.Task<Department> UpdateAsync(Department department);
    
    /// <summary>
    /// Soft deletes a department
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
