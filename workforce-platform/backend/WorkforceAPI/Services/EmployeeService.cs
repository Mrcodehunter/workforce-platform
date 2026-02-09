using System.Text.Json;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using WorkforceAPI.Exceptions;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

/// <summary>
/// Service implementation for employee-related business operations
/// </summary>
/// <remarks>
/// This service class implements the business logic for employee management.
/// It follows the Service Layer pattern, separating business logic from:
/// - Data access (handled by IEmployeeRepository)
/// - HTTP concerns (handled by Controllers)
/// - Event publishing (handled by IRabbitMqPublisher)
/// - Caching (handled by IRedisCache)
/// 
/// Key responsibilities:
/// 1. Business rule validation and enforcement
/// 2. Data transformation (Entity to DTO mapping)
/// 3. Audit trail management (before/after snapshots)
/// 4. Event publishing for event-driven architecture
/// 
/// All methods are async to support non-blocking I/O operations.
/// </remarks>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    /// <summary>
    /// Initializes a new instance of EmployeeService
    /// </summary>
    /// <param name="repository">Repository for employee data access</param>
    /// <param name="eventPublisher">Publisher for domain events (audit trail)</param>
    /// <param name="redisCache">Cache for storing audit snapshots</param>
    /// <remarks>
    /// Dependencies are injected via constructor injection, following the
    /// Dependency Inversion Principle. This makes the service testable and
    /// allows for easy mocking in unit tests.
    /// </remarks>
    public EmployeeService(IEmployeeRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
    }

    /// <summary>
    /// Retrieves all employees as a list (non-paginated)
    /// </summary>
    /// <returns>Collection of employee list DTOs</returns>
    /// <remarks>
    /// This method retrieves all employees from the repository and maps them to EmployeeListDto.
    /// The mapping includes:
    /// - Basic employee information (name, email, phone, etc.)
    /// - Department information (if assigned)
    /// - Designation information (if assigned)
    /// 
    /// Navigation properties are manually mapped to DTOs to:
    /// 1. Control what data is exposed (avoid exposing internal fields)
    /// 2. Prevent circular reference issues in JSON serialization
    /// 3. Optimize data transfer (only include necessary fields)
    /// 
    /// Note: For large datasets, this method can be slow. Use GetPagedAsync for better performance.
    /// </remarks>
    public async Task<IEnumerable<EmployeeListDto>> GetAllAsync()
    {
        // Retrieve all employees from repository (includes navigation properties via EF Core)
        var employees = await _repository.GetAllAsync();
        
        // Map Employee entities to EmployeeListDto
        // This transformation layer ensures only necessary data is exposed to the API
        return employees.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            IsActive = e.IsActive,
            Phone = e.Phone,
            AvatarUrl = e.AvatarUrl,
            // Map Department navigation property to DTO (null-safe)
            // Only include essential department fields, not the full entity
            Department = e.Department != null ? new DepartmentDto
            {
                Id = e.Department.Id,
                Name = e.Department.Name,
                Description = e.Department.Description
            } : null,
            // Map Designation navigation property to DTO (null-safe)
            // Only include essential designation fields
            Designation = e.Designation != null ? new DesignationDto
            {
                Id = e.Designation.Id,
                Title = e.Designation.Title,
                Level = e.Designation.Level,
                Description = e.Designation.Description
            } : null
        });
    }

    /// <summary>
    /// Retrieves employees with pagination support
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing employees and pagination metadata</returns>
    /// <remarks>
    /// This method provides efficient pagination for large employee lists.
    /// It validates and normalizes pagination parameters to ensure safe operation:
    /// - Page numbers less than 1 are normalized to 1
    /// - Page sizes less than 1 are normalized to 10 (default)
    /// - Page sizes greater than 100 are capped at 100 (prevents excessive data transfer and performance issues)
    /// 
    /// The repository performs the actual pagination at the database level for optimal performance.
    /// </remarks>
    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(int page, int pageSize)
    {
        // Validate and normalize pagination parameters
        // This ensures safe operation even with invalid input from the API
        if (page < 1) page = 1;  // Minimum page is 1
        if (pageSize < 1) pageSize = 10;  // Default page size
        if (pageSize > 100) pageSize = 100; // Max page size limit to prevent performance issues

        // Repository performs database-level pagination (efficient for large datasets)
        // Returns tuple: (employees for current page, total count of all employees)
        var (employees, totalCount) = await _repository.GetPagedAsync(page, pageSize);
        
        // Map Employee entities to EmployeeListDto (same mapping logic as GetAllAsync)
        var data = employees.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            IsActive = e.IsActive,
            Phone = e.Phone,
            AvatarUrl = e.AvatarUrl,
            Department = e.Department != null ? new DepartmentDto
            {
                Id = e.Department.Id,
                Name = e.Department.Name,
                Description = e.Department.Description
            } : null,
            Designation = e.Designation != null ? new DesignationDto
            {
                Id = e.Designation.Id,
                Title = e.Designation.Title,
                Level = e.Designation.Level,
                Description = e.Designation.Description
            } : null
        });

        // Return paged result with metadata for frontend pagination controls
        return new PagedResult<EmployeeListDto>
        {
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount  // Used by frontend to calculate total pages
        };
    }

    /// <summary>
    /// Retrieves a single employee by ID with full details
    /// </summary>
    /// <param name="id">The unique identifier of the employee</param>
    /// <returns>Employee detail DTO with all related information</returns>
    /// <exception cref="ValidationException">Thrown if id is empty</exception>
    /// <exception cref="EntityNotFoundException">Thrown if employee doesn't exist</exception>
    /// <remarks>
    /// This method retrieves a single employee with all related data:
    /// - Employee basic information
    /// - Department and Designation details
    /// - Project memberships (all projects the employee is assigned to)
    /// 
    /// The EmployeeDetailDto includes more information than EmployeeListDto,
    /// making it suitable for detail views in the frontend.
    /// </remarks>
    public async Task<EmployeeDetailDto> GetByIdAsync(Guid id)
    {
        // Validate input - empty GUID indicates invalid/missing ID
        if (id == Guid.Empty)
        {
            throw new ValidationException("Employee ID is required", nameof(id));
        }

        // Retrieve employee from repository (includes navigation properties via EF Core)
        var employee = await _repository.GetByIdAsync(id);
        
        // Ensure employee exists - throw exception if not found
        // This follows the "fail fast" principle and provides clear error messages
        if (employee == null)
        {
            throw new EntityNotFoundException("Employee", id);
        }

        // Map Employee entity to EmployeeDetailDto
        // This DTO includes all employee fields plus related entities
        return new EmployeeDetailDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            Salary = employee.Salary,
            JoiningDate = employee.JoiningDate,
            Phone = employee.Phone,
            Address = employee.Address,
            City = employee.City,
            Country = employee.Country,
            // Ensure Skills is never null (empty list if not set)
            Skills = employee.Skills ?? new List<string>(),
            AvatarUrl = employee.AvatarUrl,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            IsDeleted = employee.IsDeleted,
            // Map Department navigation property to DTO
            Department = employee.Department != null ? new DepartmentDto
            {
                Id = employee.Department.Id,
                Name = employee.Department.Name,
                Description = employee.Department.Description
            } : null,
            // Map Designation navigation property to DTO
            Designation = employee.Designation != null ? new DesignationDto
            {
                Id = employee.Designation.Id,
                Title = employee.Designation.Title,
                Level = employee.Designation.Level,
                Description = employee.Designation.Description
            } : null,
            // Map ProjectMembers collection to DTOs
            // This includes all projects the employee is assigned to, with their roles
            ProjectMembers = employee.ProjectMembers?.Select(pm => new ProjectMemberDto
            {
                ProjectId = pm.ProjectId,
                EmployeeId = pm.EmployeeId,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt,
                // Include project summary for each membership
                Project = pm.Project != null ? new ProjectSummaryDto
                {
                    Id = pm.Project.Id,
                    Name = pm.Project.Name,
                    Description = pm.Project.Description,
                    Status = pm.Project.Status,
                    StartDate = pm.Project.StartDate,
                    EndDate = pm.Project.EndDate
                } : null
            }).ToList() ?? new List<ProjectMemberDto>()  // Empty list if no project memberships
        };
    }

    /// <summary>
    /// Creates a new employee
    /// </summary>
    /// <param name="employee">The employee entity to create</param>
    /// <returns>The created employee with generated ID and timestamps</returns>
    /// <exception cref="ValidationException">Thrown if validation fails (handled by FluentValidation)</exception>
    /// <remarks>
    /// This method implements the complete employee creation workflow:
    /// 1. Sets system-generated fields (ID, timestamps)
    /// 2. Clears navigation properties to avoid Entity Framework tracking issues
    /// 3. Saves the employee to the database
    /// 4. Reloads the employee to get all database-generated values
    /// 5. Stores "after" snapshot in Redis for audit trail
    /// 6. Publishes EmployeeCreated event
    /// 
    /// The order of operations is critical:
    /// - Redis snapshot must be stored BEFORE publishing the event
    /// - This ensures the audit logger worker can retrieve the snapshot when processing the event
    /// - The eventId links the event to the Redis-stored snapshot
    /// </remarks>
    public async Task<Employee> CreateAsync(Employee employee)
    {
        // Business logic: Set system-generated fields
        // These fields are not provided by the client and must be set by the service
        employee.Id = Guid.NewGuid();  // Generate unique identifier
        employee.CreatedAt = DateTime.UtcNow;  // Track when entity was created
        employee.UpdatedAt = DateTime.UtcNow;  // Initially same as CreatedAt
        employee.IsDeleted = false;  // New employees are not deleted

        // Clear navigation properties to avoid Entity Framework tracking issues
        // Navigation properties should be set via foreign keys (DepartmentId, DesignationId)
        // Setting them to null prevents EF Core from trying to track/update related entities
        employee.Department = null;
        employee.Designation = null;
        employee.ProjectMembers = new List<ProjectMember>();  // Empty list (memberships added separately)
        employee.AssignedTasks = new List<TaskItem>();  // Empty list (tasks assigned separately)

        // Save employee to database
        // Repository handles the actual database insert operation
        var result = await _repository.CreateAsync(employee);
        
        // Reload employee from database to ensure we have all database-generated values
        // This is important if the database sets any default values or triggers modify the data
        var reloaded = await _repository.GetByIdAsync(result.Id);
        
        // Generate event ID for correlation with Redis snapshot
        // This ID links the event to the "after" snapshot stored in Redis
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        // This order is critical: snapshot must exist when the audit logger worker processes the event
        // The snapshot is stored with 1-hour expiration to allow time for worker processing
        if (reloaded != null)
        {
            // Serialize employee to JSON (without navigation properties to avoid circular references)
            var afterSnapshot = AuditEntitySerializer.SerializeEmployee(reloaded);
            // Store in Redis with key pattern: "audit:{eventId}:after"
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event after Redis key is set
        // The eventId in the event payload allows the audit logger worker to retrieve the snapshot
        // Event payload includes minimal data (just EmployeeId) - full data is in Redis snapshot
        await _eventPublisher.PublishEventAsync(AuditEventType.EmployeeCreated, new { EmployeeId = result.Id }, eventId);
        
        // Return reloaded employee (with all navigation properties) or fallback to result
        return reloaded ?? result;
    }

    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="employee">The employee entity with updated values</param>
    /// <returns>The updated employee with refreshed data</returns>
    /// <exception cref="ValidationException">Thrown if employee ID is empty</exception>
    /// <exception cref="EntityNotFoundException">Thrown if employee doesn't exist</exception>
    /// <remarks>
    /// This method implements a complete audit trail workflow for updates:
    /// 1. Validates employee ID and existence
    /// 2. Stores "before" snapshot in Redis (captures current state)
    /// 3. Updates only allowed properties (preserves system fields)
    /// 4. Saves changes to database
    /// 5. Retrieves updated employee
    /// 6. Stores "after" snapshot in Redis (captures new state)
    /// 7. Publishes EmployeeUpdated event
    /// 
    /// The order of operations is critical:
    /// - "Before" snapshot must be captured BEFORE any changes
    /// - "After" snapshot must be captured AFTER database save (to get final state)
    /// - Event must be published AFTER both snapshots are stored
    /// 
    /// Only updatable properties are modified - system fields like Id, CreatedAt are preserved.
    /// </remarks>
    public async Task<Employee> UpdateAsync(Employee employee)
    {
        // Validate employee ID - empty GUID indicates invalid/missing ID
        if (employee.Id == Guid.Empty)
        {
            throw new ValidationException("Employee ID is required", nameof(employee.Id));
        }

        // Retrieve existing employee from database
        // This is needed to:
        // 1. Verify employee exists
        // 2. Capture "before" snapshot for audit trail
        // 3. Update only changed properties (preserve existing values for unchanged fields)
        var existingEmployee = await _repository.GetByIdAsync(employee.Id);
        
        // Ensure employee exists - throw exception if not found
        if (existingEmployee == null)
        {
            throw new EntityNotFoundException("Employee", employee.Id);
        }

        // Generate event ID for correlation with Redis snapshots
        // This ID links the event to both "before" and "after" snapshots
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save "before" snapshot into Redis
        // This captures the current state of the employee BEFORE any changes
        // The snapshot is stored with 1-hour expiration to allow time for worker processing
        var beforeSnapshot = AuditEntitySerializer.SerializeEmployee(existingEmployee);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2: Execute business logic - Update only the properties that should be updated
        // We update the existing entity rather than replacing it to:
        // 1. Preserve system fields (Id, CreatedAt) that shouldn't be changed
        // 2. Maintain Entity Framework tracking
        // 3. Ensure only allowed properties are modified
        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Phone = employee.Phone;
        existingEmployee.Address = employee.Address;
        existingEmployee.City = employee.City;
        existingEmployee.Country = employee.Country;
        existingEmployee.Salary = employee.Salary;
        existingEmployee.JoiningDate = employee.JoiningDate;
        existingEmployee.DepartmentId = employee.DepartmentId;  // Update via foreign key, not navigation property
        existingEmployee.DesignationId = employee.DesignationId;  // Update via foreign key, not navigation property
        existingEmployee.Skills = employee.Skills;
        existingEmployee.AvatarUrl = employee.AvatarUrl;
        existingEmployee.IsActive = employee.IsActive;
        existingEmployee.UpdatedAt = DateTime.UtcNow;  // Update timestamp to reflect modification

        // Step 3: Execute DB operations - save changes to database
        var result = await _repository.UpdateAsync(existingEmployee);
        
        // Step 4: Retrieve updated data from DB
        // Reload to ensure we have the final state after all database operations/triggers
        var reloaded = await _repository.GetByIdAsync(result.Id);
        
        // Step 5: Save "after" snapshot into Redis
        // This captures the new state of the employee AFTER changes
        // The audit logger worker will compare "before" and "after" to show what changed
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeEmployee(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Step 6: Publish the event (after all operations completed)
        // The eventId in the event payload allows the audit logger worker to retrieve both snapshots
        // Event is published last to ensure snapshots are available when worker processes the event
        await _eventPublisher.PublishEventAsync(AuditEventType.EmployeeUpdated, new { EmployeeId = employee.Id }, eventId);
        
        // Return reloaded employee (with all navigation properties) or fallback to result
        return reloaded ?? result;
    }

    /// <summary>
    /// Deletes an employee (soft delete)
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete</param>
    /// <remarks>
    /// This method performs a soft delete (sets IsDeleted flag) rather than physically
    /// removing the record. This approach:
    /// 1. Preserves data for audit and recovery purposes
    /// 2. Maintains referential integrity (related records can still reference the employee)
    /// 3. Allows data recovery if deletion was accidental
    /// 
    /// If the employee exists:
    /// 1. Captures "before" snapshot in Redis (for audit trail)
    /// 2. Performs soft delete in database
    /// 3. Publishes EmployeeDeleted event
    /// 
    /// If the employee doesn't exist, the delete operation still proceeds (idempotent operation).
    /// </remarks>
    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        // This allows the audit trail to show what was deleted
        var existingEmployee = await _repository.GetByIdAsync(id);
        
        if (existingEmployee != null)
        {
            // Generate event ID for correlation with Redis snapshot
            var eventId = Guid.NewGuid().ToString();
            
            // Step 1: Save "before" snapshot into Redis
            // This captures the employee state before deletion
            // For deletes, we only need "before" snapshot (no "after" state exists)
            var beforeSnapshot = AuditEntitySerializer.SerializeEmployee(existingEmployee);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            // Repository performs soft delete (sets IsDeleted = true)
            // This is idempotent - calling multiple times has the same effect
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            // The eventId allows the audit logger worker to retrieve the "before" snapshot
            // Event is published last to ensure snapshot is available when worker processes the event
            await _eventPublisher.PublishEventAsync(AuditEventType.EmployeeDeleted, new { EmployeeId = id }, eventId);
        }
        else
        {
            // Employee doesn't exist - still call delete for idempotency
            // This ensures the method behaves consistently whether employee exists or not
            await _repository.DeleteAsync(id);
        }
    }
}
