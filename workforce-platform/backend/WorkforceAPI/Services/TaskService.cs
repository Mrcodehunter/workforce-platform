using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.EventPublisher;

namespace WorkforceAPI.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public TaskService(ITaskRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
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

        // Clear navigation properties to avoid tracking issues
        task.Project = null;
        task.AssignedToEmployee = null;

        var result = await _repository.CreateAsync(task);
        
        // Reload with navigation properties for event
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("task.created", new { TaskId = result.Id, ProjectId = result.ProjectId });
        
        return reloaded ?? result;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        var existingTask = await _repository.GetByIdAsync(task.Id);
        if (existingTask == null)
        {
            throw new InvalidOperationException($"Task with ID {task.Id} not found");
        }

        // Update only the properties that should be updated
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.ProjectId = task.ProjectId;
        existingTask.AssignedToEmployeeId = task.AssignedToEmployeeId;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingTask);
        
        // Reload with navigation properties
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("task.updated", new { TaskId = result.Id, ProjectId = result.ProjectId });
        
        return reloaded ?? result;
    }

    public async Task<TaskItem> UpdateTaskStatusAsync(Guid taskId, string status)
    {
        var task = await _repository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {taskId} not found");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(task);
        
        // Reload with navigation properties
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("task.status.updated", new { TaskId = result.Id, Status = status, ProjectId = result.ProjectId });
        
        return reloaded ?? result;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("task.deleted", new { TaskId = id });
    }
}
