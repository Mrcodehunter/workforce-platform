using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskListDto>> GetAllAsync();
    Task<TaskDetailDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskListDto>> GetTasksByProjectIdAsync(Guid projectId);
    Task<IEnumerable<TaskListDto>> GetTasksByEmployeeIdAsync(Guid employeeId);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task<TaskItem> UpdateTaskStatusAsync(Guid taskId, string status);
    Task DeleteAsync(Guid id);
}
