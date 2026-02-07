using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using WorkforceAPI.EventPublisher;

namespace WorkforceAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public EmployeeService(IEmployeeRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        var result = await _repository.CreateAsync(employee);
        await _eventPublisher.PublishEventAsync("employee.created", new { EmployeeId = result.Id });
        return result;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        var result = await _repository.UpdateAsync(employee);
        await _eventPublisher.PublishEventAsync("employee.updated", new { EmployeeId = result.Id });
        return result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("employee.deleted", new { EmployeeId = id });
    }
}
