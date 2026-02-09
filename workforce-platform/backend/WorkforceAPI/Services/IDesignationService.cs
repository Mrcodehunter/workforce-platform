using WorkforceAPI.Models;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for designation-related business operations
/// </summary>
public interface IDesignationService
{
    /// <summary>
    /// Retrieves all designations
    /// </summary>
    Task<IEnumerable<Designation>> GetAllAsync();
    
    /// <summary>
    /// Retrieves a designation by ID
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
    /// Deletes a designation (soft delete)
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
