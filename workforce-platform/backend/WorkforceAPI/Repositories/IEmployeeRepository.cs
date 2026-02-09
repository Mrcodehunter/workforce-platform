using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for employee data access operations
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// Gets all employees
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<Employee>> GetAllAsync();
    
    /// <summary>
    /// Gets paginated employees
    /// </summary>
    System.Threading.Tasks.Task<(IEnumerable<Employee> Data, int TotalCount)> GetPagedAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets employee by ID
    /// </summary>
    System.Threading.Tasks.Task<Employee?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new employee
    /// </summary>
    System.Threading.Tasks.Task<Employee> CreateAsync(Employee employee);
    
    /// <summary>
    /// Updates an existing employee
    /// </summary>
    System.Threading.Tasks.Task<Employee> UpdateAsync(Employee employee);
    
    /// <summary>
    /// Soft deletes an employee
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
