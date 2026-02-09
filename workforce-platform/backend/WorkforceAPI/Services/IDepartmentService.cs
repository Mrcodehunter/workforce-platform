using WorkforceAPI.Models;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for department-related business operations
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Retrieves all departments
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<Department>> GetAllAsync();
    
    /// <summary>
    /// Retrieves a department by ID
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
    /// Deletes a department (soft delete)
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
