using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using WorkforceAPI.EventPublisher;

namespace WorkforceAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public ProjectService(IProjectRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        var result = await _repository.CreateAsync(project);
        await _eventPublisher.PublishEventAsync("project.created", new { ProjectId = result.Id });
        return result;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        var result = await _repository.UpdateAsync(project);
        await _eventPublisher.PublishEventAsync("project.updated", new { ProjectId = result.Id });
        return result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("project.deleted", new { ProjectId = id });
    }
}
