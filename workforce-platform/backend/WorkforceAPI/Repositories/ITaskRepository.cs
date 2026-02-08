using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<TaskItem>> GetByEmployeeIdAsync(Guid employeeId);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
