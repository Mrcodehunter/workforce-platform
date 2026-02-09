using System.Text.Json;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    public TaskService(ITaskRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
    }

    public async Task<IEnumerable<TaskListDto>> GetAllAsync()
    {
        var tasks = await _repository.GetAllAsync();
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
            Project = t.Project != null ? new ProjectSummaryDto
            {
                Id = t.Project.Id,
                Name = t.Project.Name,
                Description = t.Project.Description,
                Status = t.Project.Status,
                StartDate = t.Project.StartDate,
                EndDate = t.Project.EndDate
            } : null,
            AssignedToEmployee = t.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = t.AssignedToEmployee.Id,
                FirstName = t.AssignedToEmployee.FirstName,
                LastName = t.AssignedToEmployee.LastName,
                Email = t.AssignedToEmployee.Email
            } : null
        });
    }

    public async Task<TaskDetailDto?> GetByIdAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task == null)
            return null;

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
            Project = task.Project != null ? new ProjectSummaryDto
            {
                Id = task.Project.Id,
                Name = task.Project.Name,
                Description = task.Project.Description,
                Status = task.Project.Status,
                StartDate = task.Project.StartDate,
                EndDate = task.Project.EndDate
            } : null,
            AssignedToEmployee = task.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = task.AssignedToEmployee.Id,
                FirstName = task.AssignedToEmployee.FirstName,
                LastName = task.AssignedToEmployee.LastName,
                Email = task.AssignedToEmployee.Email
            } : null
        };
    }

    public async Task<IEnumerable<TaskListDto>> GetTasksByProjectIdAsync(Guid projectId)
    {
        var tasks = await _repository.GetByProjectIdAsync(projectId);
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
            AssignedToEmployee = t.AssignedToEmployee != null ? new EmployeeBasicDto
            {
                Id = t.AssignedToEmployee.Id,
                FirstName = t.AssignedToEmployee.FirstName,
                LastName = t.AssignedToEmployee.LastName,
                Email = t.AssignedToEmployee.Email
            } : null
        });
    }

    public async Task<IEnumerable<TaskListDto>> GetTasksByEmployeeIdAsync(Guid employeeId)
    {
        var tasks = await _repository.GetByEmployeeIdAsync(employeeId);
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

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        // Business logic: Set timestamps and IDs
        task.Id = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        task.IsDeleted = false;
        
        // Set default status if not provided
        if (string.IsNullOrWhiteSpace(task.Status))
        {
            task.Status = "ToDo";
        }

        // Clear navigation properties to avoid tracking issues
        task.Project = null;
        task.AssignedToEmployee = null;

        var result = await _repository.CreateAsync(task);
        
        // Reload with navigation properties for event
        var reloaded = await _repository.GetByIdAsync(result.Id);
        
        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskCreated, new { TaskId = result.Id, ProjectId = result.ProjectId }, eventId);
        
        return reloaded ?? result;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        var existingTask = await _repository.GetByIdAsync(task.Id);
        if (existingTask == null)
        {
            throw new InvalidOperationException($"Task with ID {task.Id} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(existingTask);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2: Execute business logic - Update only the properties that should be updated
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.ProjectId = task.ProjectId;
        existingTask.AssignedToEmployeeId = task.AssignedToEmployeeId;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.UpdatedAt = DateTime.UtcNow;

        // Step 3: Execute DB operations
        var result = await _repository.UpdateAsync(existingTask);
        
        // Step 4: Retrieve updated data from DB
        var reloaded = await _repository.GetByIdAsync(result.Id);
        
        // Step 5: Save afterSnapshot into Redis
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskUpdated, new { TaskId = task.Id, ProjectId = task.ProjectId }, eventId);
        
        return reloaded ?? result;
    }

    public async Task<TaskItem> UpdateTaskStatusAsync(Guid taskId, string status)
    {
        var task = await _repository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {taskId} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "before" snapshot and store in Redis BEFORE publishing event
        var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(task);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(task);
        
        // Reload with navigation properties and capture "after" snapshot
        var reloaded = await _repository.GetByIdAsync(result.Id);
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeTaskItem(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event AFTER both before and after snapshots are stored in Redis
        await _eventPublisher.PublishEventAsync(AuditEventType.TaskStatusUpdated, new { TaskId = taskId, Status = status, ProjectId = task.ProjectId }, eventId);
        
        return reloaded ?? result;
    }

    public async Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        var existingTask = await _repository.GetByIdAsync(id);
        if (existingTask != null)
        {
            // Generate event ID first
            var eventId = Guid.NewGuid().ToString();
            
            // Step 1: Save beforeSnapshot into Redis
            var beforeSnapshot = AuditEntitySerializer.SerializeTaskItem(existingTask);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            await _eventPublisher.PublishEventAsync(AuditEventType.TaskDeleted, new { TaskId = id }, eventId);
        }
        else
        {
            await _repository.DeleteAsync(id);
        }
    }
}
