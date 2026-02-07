using WorkforceAPI.Models;
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

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        var result = await _repository.CreateAsync(task);
        await _eventPublisher.PublishEventAsync("task.created", new { TaskId = result.Id });
        return result;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        var result = await _repository.UpdateAsync(task);
        await _eventPublisher.PublishEventAsync("task.updated", new { TaskId = result.Id });
        return result;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("task.deleted", new { TaskId = id });
    }
}
