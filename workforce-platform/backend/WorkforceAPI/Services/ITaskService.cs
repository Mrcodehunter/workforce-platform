using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for task-related business operations
/// </summary>
/// <remarks>
/// This interface defines the contract for task management operations.
/// It separates business logic from data access (repository pattern) and HTTP concerns (controllers).
/// 
/// All methods are async to support non-blocking I/O operations.
/// Methods return DTOs (Data Transfer Objects) rather than domain entities to:
/// 1. Control what data is exposed to the API layer
/// 2. Avoid circular reference issues in JSON serialization
/// 3. Optimize data transfer (only send necessary fields)
/// </remarks>
public interface ITaskService
{
    /// <summary>
    /// Retrieves all tasks as a list
    /// </summary>
    /// <returns>Collection of task list DTOs</returns>
    /// <remarks>
    /// Returns TaskListDto which includes basic task info plus Project and AssignedToEmployee summaries.
    /// </remarks>
    Task<IEnumerable<TaskListDto>> GetAllAsync();
    
    /// <summary>
    /// Retrieves a single task by ID with full details
    /// </summary>
    /// <param name="id">The unique identifier of the task</param>
    /// <returns>Task detail DTO, or null if not found</returns>
    /// <remarks>
    /// Returns TaskDetailDto which includes all task properties plus related entity summaries.
    /// </remarks>
    Task<TaskDetailDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all tasks for a specific project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <returns>Collection of task list DTOs for the project</returns>
    /// <remarks>
    /// Used to display all tasks on a project detail page.
    /// </remarks>
    Task<IEnumerable<TaskListDto>> GetTasksByProjectIdAsync(Guid projectId);
    
    /// <summary>
    /// Retrieves all tasks assigned to a specific employee
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee</param>
    /// <returns>Collection of task list DTOs assigned to the employee</returns>
    /// <remarks>
    /// Used to display all tasks on an employee detail page or task board filtered by assignee.
    /// </remarks>
    Task<IEnumerable<TaskListDto>> GetTasksByEmployeeIdAsync(Guid employeeId);
    
    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="task">The task entity to create</param>
    /// <returns>The created task with generated ID and timestamps</returns>
    /// <exception cref="ValidationException">Thrown if validation fails</exception>
    /// <remarks>
    /// Sets system-generated fields (ID, timestamps), clears navigation properties,
    /// saves to database, and publishes TaskCreated audit event.
    /// </remarks>
    Task<TaskItem> CreateAsync(TaskItem task);
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="task">The task entity with updated values</param>
    /// <returns>The updated task</returns>
    /// <exception cref="InvalidOperationException">Thrown if task doesn't exist</exception>
    /// <remarks>
    /// Implements complete audit trail workflow with before/after snapshots.
    /// </remarks>
    Task<TaskItem> UpdateAsync(TaskItem task);
    
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
    /// Validates status value and publishes TaskStatusUpdated audit event.
    /// </remarks>
    Task<TaskItem> UpdateTaskStatusAsync(Guid taskId, string status);
    
    /// <summary>
    /// Deletes a task (soft delete)
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete</param>
    /// <remarks>
    /// Performs soft delete and publishes TaskDeleted audit event.
    /// </remarks>
    Task DeleteAsync(Guid id);
}
