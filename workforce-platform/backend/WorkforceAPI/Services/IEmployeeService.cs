using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for employee-related business operations
/// </summary>
/// <remarks>
/// This interface defines the contract for employee management operations.
/// It separates business logic from data access (repository pattern) and HTTP concerns (controllers).
/// 
/// All methods are async to support non-blocking I/O operations.
/// Methods return DTOs (Data Transfer Objects) rather than domain entities to:
/// 1. Control what data is exposed to the API layer
/// 2. Avoid circular reference issues in JSON serialization
/// 3. Optimize data transfer (only send necessary fields)
/// </remarks>
public interface IEmployeeService
{
    /// <summary>
    /// Retrieves all employees as a list (non-paginated)
    /// </summary>
    /// <returns>Collection of employee list DTOs</returns>
    /// <remarks>
    /// This method is provided for backward compatibility and simple use cases.
    /// For large datasets, use GetPagedAsync instead to avoid performance issues.
    /// 
    /// Returns EmployeeListDto which includes basic employee info plus Department and Designation.
    /// </remarks>
    System.Threading.Tasks.Task<IEnumerable<EmployeeListDto>> GetAllAsync();
    
    /// <summary>
    /// Retrieves employees with pagination support
    /// </summary>
    /// <param name="page">Page number (1-based, minimum 1)</param>
    /// <param name="pageSize">Number of items per page (1-100, default 10)</param>
    /// <returns>Paged result containing employees and pagination metadata</returns>
    /// <remarks>
    /// This method provides efficient pagination for large employee lists.
    /// Parameters are automatically validated and normalized:
    /// - Page less than 1 is set to 1
    /// - PageSize less than 1 is set to 10
    /// - PageSize greater than 100 is capped at 100 (prevents excessive data transfer)
    /// 
    /// Returns PagedResult&lt;EmployeeListDto&gt; which includes:
    /// - Data: The employee list for the requested page
    /// - Page: Current page number
    /// - PageSize: Items per page
    /// - TotalCount: Total number of employees (for calculating total pages)
    /// </remarks>
    System.Threading.Tasks.Task<PagedResult<EmployeeListDto>> GetPagedAsync(int page, int pageSize);
    
    /// <summary>
    /// Retrieves a single employee by ID with full details
    /// </summary>
    /// <param name="id">The unique identifier of the employee</param>
    /// <returns>Employee detail DTO with all related information</returns>
    /// <exception cref="ValidationException">Thrown if id is empty</exception>
    /// <exception cref="EntityNotFoundException">Thrown if employee with the given ID doesn't exist</exception>
    /// <remarks>
    /// Returns EmployeeDetailDto which includes:
    /// - All employee properties
    /// - Department and Designation details
    /// - Project memberships (projects the employee is assigned to)
    /// - Timestamps and metadata
    /// </remarks>
    System.Threading.Tasks.Task<EmployeeDetailDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new employee
    /// </summary>
    /// <param name="employee">The employee entity to create</param>
    /// <returns>The created employee with generated ID and timestamps</returns>
    /// <exception cref="ValidationException">Thrown if validation fails</exception>
    /// <remarks>
    /// This method:
    /// 1. Generates a new GUID for the employee ID
    /// 2. Sets CreatedAt and UpdatedAt timestamps to UTC now
    /// 3. Clears navigation properties to avoid Entity Framework tracking issues
    /// 4. Saves the employee to the database
    /// 5. Publishes an EmployeeCreated audit event with "after" snapshot
    /// 
    /// The employee is reloaded from the database after creation to ensure
    /// all database-generated values (if any) are included in the response.
    /// </remarks>
    System.Threading.Tasks.Task<Employee> CreateAsync(Employee employee);
    
    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="employee">The employee entity with updated values</param>
    /// <returns>The updated employee with refreshed data</returns>
    /// <exception cref="ValidationException">Thrown if employee ID is empty</exception>
    /// <exception cref="EntityNotFoundException">Thrown if employee doesn't exist</exception>
    /// <remarks>
    /// This method implements a complete audit trail workflow:
    /// 1. Retrieves existing employee from database
    /// 2. Stores "before" snapshot in Redis (for audit trail)
    /// 3. Updates only allowed properties (prevents overwriting system fields like CreatedAt)
    /// 4. Saves changes to database
    /// 5. Retrieves updated employee
    /// 6. Stores "after" snapshot in Redis
    /// 7. Publishes EmployeeUpdated event
    /// 
    /// Only updatable properties are modified - system fields like Id and CreatedAt are preserved.
    /// The UpdatedAt timestamp is automatically set to UTC now.
    /// </remarks>
    System.Threading.Tasks.Task<Employee> UpdateAsync(Employee employee);
    
    /// <summary>
    /// Deletes an employee (soft delete)
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete</param>
    /// <exception cref="EntityNotFoundException">Thrown if employee doesn't exist</exception>
    /// <remarks>
    /// This method performs a soft delete (sets IsDeleted flag) rather than
    /// physically removing the record from the database. This allows:
    /// - Data recovery if deletion was accidental
    /// - Historical data preservation
    /// - Referential integrity maintenance
    /// 
    /// If the employee exists, a "before" snapshot is stored in Redis and
    /// an EmployeeDeleted event is published for audit trail purposes.
    /// </remarks>
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
