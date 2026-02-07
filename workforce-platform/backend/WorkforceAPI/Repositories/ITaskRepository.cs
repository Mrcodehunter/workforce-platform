using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
