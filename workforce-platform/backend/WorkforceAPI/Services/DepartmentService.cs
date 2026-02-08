using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public DepartmentService(IDepartmentRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        var result = await _repository.CreateAsync(department);
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentCreated, new { DepartmentId = result.Id });
        return result;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        var result = await _repository.UpdateAsync(department);
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentUpdated, new { DepartmentId = result.Id });
        return result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentDeleted, new { DepartmentId = id });
    }
}
