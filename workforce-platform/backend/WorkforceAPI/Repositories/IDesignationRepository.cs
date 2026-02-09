using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for designation data access operations
/// </summary>
public interface IDesignationRepository
{
    /// <summary>
    /// Gets all designations
    /// </summary>
    Task<IEnumerable<Designation>> GetAllAsync();
    
    /// <summary>
    /// Gets designation by ID
    /// </summary>
    Task<Designation?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new designation
    /// </summary>
    Task<Designation> CreateAsync(Designation designation);
    
    /// <summary>
    /// Updates an existing designation
    /// </summary>
    Task<Designation> UpdateAsync(Designation designation);
    
    /// <summary>
    /// Soft deletes a designation
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
