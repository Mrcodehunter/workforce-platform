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
/// Service implementation for task-related business operations
/// </summary>
/// <remarks>
/// This service class implements the business logic for task management.
/// It follows the Service Layer pattern, separating business logic from:
/// - Data access (handled by ITaskRepository)
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
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    /// <summary>
    /// Initializes a new instance of TaskService
    /// </summary>
    /// <param name="repository">Repository for task data access</param>
    /// <param name="eventPublisher">Publisher for domain events (audit trail)</param>
    /// <param name="redisCache">Cache for storing audit snapshots</param>
    /// <remarks>
    /// Dependencies are injected via constructor injection, following the
    /// Dependency Inversion Principle. This makes the service testable and
    /// allows for easy mocking in unit tests.
    /// </remarks>
    public TaskService(ITaskRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
    }

    /// <summary>
    /// Retrieves all tasks as a list
    /// </summary>
    /// <returns>Collection of task list DTOs</returns>
    /// <remarks>
    /// This method retrieves all tasks from the repository and maps them to TaskListDto.
    /// The mapping includes:
    /// - Basic task information (title, description, status, priority, due date)
    /// - Project summary information (if assigned to a project)
    /// - Assigned employee summary information (if assigned to an employee)
    /// 
    /// Navigation properties are manually mapped to DTOs to:
    /// 1. Control what data is exposed (avoid exposing internal fields)
    /// 2. Prevent circular reference issues in JSON serialization
    /// 3. Optimize data transfer (only include necessary fields)
    /// </remarks>
    public async Task<IEnumerable<TaskListDto>> GetAllAsync()
    {
        // Retrieve all tasks from repository (includes navigation properties via EF Core)
        var tasks = await _repository.GetAllAsync();
        
        // Map TaskItem entities to TaskListDto
        // This transformation layer ensures only necessary data is exposed to the API
        return tasks.Select(t => new TaskListDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            AssignedToEmployeeId = t.AssignedToEmployeeId,
            // Map Project navigation property to DTO (null-safe)
            // Only include essential project fields, not the full entity
            Project = t.Project != null ? new ProjectSummaryDto
            {
                Id = t.Project.Id,
                Name = t.Project.Name,
                Description = t.Project.Description,
                Status = t.Project.Status,
                StartDate = t.Project.StartDate,
                EndDate = t.Project.EndDate
            } : null,
            // Map AssignedToEmployee navigation property to DTO (null-safe)
            // Only include essential employee fields
            AssignedToEmployee = t.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = t.AssignedToEmployee.Id,
                FirstName = t.AssignedToEmployee.FirstName,
                LastName = t.AssignedToEmployee.LastName,
                Email = t.AssignedToEmployee.Email
            } : null
        });
    }

    /// <summary>
    /// Retrieves a single task by ID with full details
    /// </summary>
    /// <param name="id">The unique identifier of the task</param>
    /// <returns>Task detail DTO, or null if not found</returns>
    /// <remarks>
    /// This method retrieves a single task with all related data:
    /// - Task basic information
    /// - Project summary details
    /// - Assigned employee summary details
    /// - Timestamps and metadata
    /// 
    /// The TaskDetailDto includes more information than TaskListDto,
    /// making it suitable for detail views in the frontend.
    /// </remarks>
    public async Task<TaskDetailDto?> GetByIdAsync(Guid id)
    {
        // Retrieve task from repository (includes navigation properties via EF Core)
        var task = await _repository.GetByIdAsync(id);
        
        // Return null if task doesn't exist (caller handles null case)
        if (task == null)
            return null;

        // Map TaskItem entity to TaskDetailDto
        // This DTO includes all task fields plus related entities
        return new TaskDetailDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            AssignedToEmployeeId = task.AssignedToEmployeeId,
            // Map Project navigation property to DTO
            Project = task.Project != null ? new ProjectSummaryDto
            {
                Id = task.Project.Id,
                Name = task.Project.Name,
                Description = task.Project.Description,
                Status = task.Project.Status,
                StartDate = task.Project.StartDate,
                EndDate = task.Project.EndDate
            } : null,
            // Map AssignedToEmployee navigation property to DTO
            AssignedToEmployee = task.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = task.AssignedToEmployee.Id,
                FirstName = task.AssignedToEmployee.FirstName,
                LastName = task.AssignedToEmployee.LastName,
                Email = task.AssignedToEmployee.Email
            } : null
        };
    }

    /// <summary>
    /// Retrieves all tasks for a specific project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Collection of task list DTOs for the project</returns>
    /// <remarks>
    /// This method retrieves all tasks that belong to a specific project.
    /// Used to display all tasks on a project detail page.
    /// 
    /// The mapping includes assigned employee information but not project information
    /// (since all tasks belong to the same project, project info is redundant).
    /// </remarks>
    public async Task<IEnumerable<TaskListDto>> GetTasksByProjectIdAsync(Guid projectId)
    {
        // Retrieve tasks for the specified project from repository
        var tasks = await _repository.GetByProjectIdAsync(projectId);
        
        // Map TaskItem entities to TaskListDto
        // Note: Project information is not included since all tasks belong to the same project
        return tasks.Select(t => new TaskListDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            AssignedToEmployeeId = t.AssignedToEmployeeId,
            // Include assigned employee information (useful for task board displays)
            AssignedToEmployee = t.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = t.AssignedToEmployee.Id,
                FirstName = t.AssignedToEmployee.FirstName,
                LastName = t.AssignedToEmployee.LastName,
                Email = t.AssignedToEmployee.Email
            } : null
        });
    }

    /// <summary>
    /// Retrieves all tasks assigned to a specific employee
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee</param>
    /// <returns>Collection of task list DTOs assigned to the employee</returns>
    /// <remarks>
    /// This method retrieves all tasks assigned to a specific employee.
    /// Used to display all tasks on an employee detail page or task board filtered by assignee.
    /// 
    /// The mapping includes project information but not assigned employee information
    /// (since all tasks are assigned to the same employee, employee info is redundant).
    /// </remarks>
    public async Task<IEnumerable<TaskListDto>> GetTasksByEmployeeIdAsync(Guid employeeId)
    {
        // Retrieve tasks assigned to the specified employee from repository
        var tasks = await _repository.GetByEmployeeIdAsync(employeeId);
        
        // Map TaskItem entities to TaskListDto
        // Note: AssignedToEmployee information is not included since all tasks are assigned to the same employee
        return tasks.Select(t => new TaskListDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            AssignedToEmployeeId = t.AssignedToEmployeeId,
            // Include project information (useful for showing which project each task belongs to)
            Project = t.Project != null ? new ProjectSummaryDto
            {
                Id = t.Project.Id,
                Name = t.Project.Name,
                Description = t.Project.Description,
                Status = t.Project.Status,
                StartDate = t.Project.StartDate,
                EndDate = t.Project.EndDate
            } : null
        });
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="task">The task entity to create</param>
    /// <returns>The created task with generated ID and timestamps</returns>
    /// <exception cref="ValidationException">Thrown if validation fails (handled by FluentValidation)</exception>
    /// <remarks>
    /// This method implements the complete task creation workflow:
    /// 1. Sets system-generated fields (ID, timestamps, default status)
    /// 2. Clears navigation properties to avoid Entity Framework tracking issues
    /// 3. Saves the task to the database
    /// 4. Reloads the task to get all database-generated values
    /// 5. Stores "after" snapshot in Redis for audit trail
    /// 6. Publishes TaskCreated event
    /// 
    /// The order of operations is critical:
    /// - Redis snapshot must be stored BEFORE publishing the event
    /// - This ensures the audit logger worker can retrieve the snapshot when processing the event
    /// - The eventId links the event to the Redis-stored snapshot
    /// </remarks>
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        // Business logic: Set system-generated fields
        // These fields are not provided by the client and must be set by the service
        task.Id = Guid.NewGuid();  // Generate unique identifier
        task.CreatedAt = DateTime.UtcNow;  // Track when entity was created
        task.UpdatedAt = DateTime.UtcNow;  // Initially same as CreatedAt
        task.IsDeleted = false;  // New tasks are not deleted
        
        // Set default status if not provided
        // Business rule: New tasks default to "ToDo" status
        if (string.IsNullOrWhiteSpace(task.Status))
        {
            task.Status = "ToDo";
        }

        // Clear navigation properties to avoid Entity Framework tracking issues
        // Navigation properties should be set via foreign keys (ProjectId, AssignedToEmployeeId)
        // Setting them to null prevents EF Core from trying to track/update related entities
        task.Project = null;
        task.AssignedToEmployee = null;

        // Save task to database
        // Repository handles the actual database insert operation
        var result = await _repository.CreateAsync(task);
        
        // Reload task from database to ensure we have all database-generated values
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
            // Serialize task to JSON (without navigation properties to avoid circular references)
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            // Store in Redis with key pattern: "audit:{eventId}:after"
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event after Redis key is set
        // The eventId in the event payload allows the audit logger worker to retrieve the snapshot
        // Event payload includes TaskId and ProjectId for context
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskCreated, new { TaskId = result.Id, ProjectId = result.ProjectId }, eventId);
        
        // Return reloaded task (with all navigation properties) or fallback to result
        return reloaded ?? result;
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="task">The task entity with updated values</param>
    /// <returns>The updated task with refreshed data</returns>
    /// <exception cref="InvalidOperationException">Thrown if task doesn't exist</exception>
    /// <remarks>
    /// This method implements a complete audit trail workflow for updates:
    /// 1. Validates task existence
    /// 2. Stores "before" snapshot in Redis (captures current state)
    /// 3. Updates only allowed properties (preserves system fields)
    /// 4. Saves changes to database
    /// 5. Retrieves updated task
    /// 6. Stores "after" snapshot in Redis (captures new state)
    /// 7. Publishes TaskUpdated event
    /// 
    /// The order of operations is critical:
    /// - "Before" snapshot must be captured BEFORE any changes
    /// - "After" snapshot must be captured AFTER database save (to get final state)
    /// - Event must be published AFTER both snapshots are stored
    /// 
    /// Only updatable properties are modified - system fields like Id, CreatedAt are preserved.
    /// </remarks>
    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        // Retrieve existing task from database
        // This is needed to:
        // 1. Verify task exists
        // 2. Capture "before" snapshot for audit trail
        // 3. Update only changed properties (preserve existing values for unchanged fields)
        var existingTask = await _repository.GetByIdAsync(task.Id);
        
        // Ensure task exists - throw exception if not found
        if (existingTask == null)
        {
            throw new InvalidOperationException($"Task with ID {task.Id} not found");
        }

        // Generate event ID for correlation with Redis snapshots
        // This ID links the event to both "before" and "after" snapshots
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save "before" snapshot into Redis
        // This captures the current state of the task BEFORE any changes
        // The snapshot is stored with 1-hour expiration to allow time for worker processing
        var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(existingTask);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2: Execute business logic - Update only the properties that should be updated
        // We update the existing entity rather than replacing it to:
        // 1. Preserve system fields (Id, CreatedAt) that shouldn't be changed
        // 2. Maintain Entity Framework tracking
        // 3. Ensure only allowed properties are modified
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.ProjectId = task.ProjectId;  // Update via foreign key, not navigation property
        existingTask.AssignedToEmployeeId = task.AssignedToEmployeeId;  // Update via foreign key, not navigation property
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.UpdatedAt = DateTime.UtcNow;  // Update timestamp to reflect modification

        // Step 3: Execute DB operations - save changes to database
        var result = await _repository.UpdateAsync(existingTask);
        
        // Step 4: Retrieve updated data from DB
        // Reload to ensure we have the final state after all database operations/triggers
        var reloaded = await _repository.GetByIdAsync(result.Id);
        
        // Step 5: Save "after" snapshot into Redis
        // This captures the new state of the task AFTER changes
        // The audit logger worker will compare "before" and "after" to show what changed
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Step 6: Publish the event (after all operations completed)
        // The eventId in the event payload allows the audit logger worker to retrieve both snapshots
        // Event is published last to ensure snapshots are available when worker processes the event
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskUpdated, new { TaskId = task.Id, ProjectId = task.ProjectId }, eventId);
        
        // Return reloaded task (with all navigation properties) or fallback to result
        return reloaded ?? result;
    }

    /// <summary>
    /// Updates only the status of a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="status">The new status value</param>
    /// <returns>The updated task</returns>
    /// <exception cref="ValidationException">Thrown if status is invalid</exception>
    /// <exception cref="EntityNotFoundException">Thrown if task doesn't exist</exception>
    /// <remarks>
    /// This is a specialized method for status updates (e.g., from task board drag-and-drop).
    /// It validates the status value against allowed values and implements the complete audit trail workflow.
    /// 
    /// The workflow:
    /// 1. Validates input parameters
    /// 2. Validates status value against allowed values
    /// 3. Retrieves existing task
    /// 4. Stores "before" snapshot in Redis
    /// 5. Updates status and timestamp
    /// 6. Saves to database
    /// 7. Stores "after" snapshot in Redis
    /// 8. Publishes TaskStatusUpdated event
    /// 
    /// This method publishes a different event type (TaskStatusUpdated) than UpdateAsync (TaskUpdated)
    /// to distinguish status-only changes from full updates.
    /// </remarks>
    public async Task<TaskItem> UpdateTaskStatusAsync(Guid taskId, string status)
    {
        // Validate input parameters
        if (taskId == Guid.Empty)
        {
            throw new ValidationException("Task ID is required", nameof(taskId));
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Status is required", nameof(status));
        }

        // Validate status value against allowed values
        // Business rule: Status must be one of the predefined values
        var validStatuses = new[] { "ToDo", "InProgress", "InReview", "Done", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new ValidationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", nameof(status), status);
        }

        // Retrieve existing task to verify it exists and capture "before" snapshot
        var task = await _repository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new EntityNotFoundException("Task", taskId);
        }

        // Generate event ID for correlation with Redis snapshots
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "before" snapshot and store in Redis BEFORE making changes
        var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(task);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Update only status and timestamp (preserve other fields)
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        // Save changes to database
        var result = await _repository.UpdateAsync(task);
        
        // Reload with navigation properties and capture "after" snapshot
        var reloaded = await _repository.GetByIdAsync(result.Id);
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event AFTER both before and after snapshots are stored in Redis
        // Use TaskStatusUpdated event type to distinguish from full updates
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskStatusUpdated, new { TaskId = taskId, Status = status, ProjectId = task.ProjectId }, eventId);
        
        return reloaded ?? result;
    }

    /// <summary>
    /// Deletes a task (soft delete)
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete</param>
    /// <remarks>
    /// This method performs a soft delete (sets IsDeleted flag) and implements the audit trail workflow.
    /// 
    /// The workflow:
    /// 1. Retrieves existing task
    /// 2. Stores "before" snapshot in Redis (captures state before deletion)
    /// 3. Performs soft delete (sets IsDeleted = true)
    /// 4. Publishes TaskDeleted event
    /// 
    /// Note: For soft deletes, there is no "after" snapshot (entity is deleted).
    /// The audit trail will show the entity state before deletion.
    /// </remarks>
    public async Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        // This is important for audit trail - we need to record what was deleted
        var existingTask = await _repository.GetByIdAsync(id);
        if (existingTask != null)
        {
            // Generate event ID for correlation with Redis snapshot
            var eventId = Guid.NewGuid().ToString();
            
            // Step 1: Save "before" snapshot into Redis
            // This captures the task state before deletion for audit trail
            var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(existingTask);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            // Repository performs soft delete (sets IsDeleted = true)
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            // No "after" snapshot for delete operations (entity no longer exists)
            await _eventPublisher.PublishEventAsync(AuditEventType.TaskDeleted, new { TaskId = id }, eventId);
        }
        else
        {
            // If task doesn't exist, still attempt delete (idempotent operation)
            // This allows delete operations to be safely retried
            await _repository.DeleteAsync(id);
        }
    }
}
