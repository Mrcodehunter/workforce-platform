using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    public DepartmentService(IDepartmentRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
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
        
        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        var afterSnapshot = AuditEntitySerializer.SerializeDepartment(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentCreated, new { DepartmentId = result.Id }, eventId);
        
        return result;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        // Get existing department for "before" snapshot
        var existingDepartment = await _repository.GetByIdAsync(department.Id);
        if (existingDepartment == null)
        {
            throw new InvalidOperationException($"Department with ID {department.Id} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "before" snapshot and store in Redis BEFORE publishing event
        var beforeSnapshot = AuditEntitySerializer.SerializeDepartment(existingDepartment);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentUpdated, new { DepartmentId = department.Id }, eventId);

        var result = await _repository.UpdateAsync(department);
        
        // Capture "after" snapshot
        var afterSnapshot = AuditEntitySerializer.SerializeDepartment(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        return result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        var existingDepartment = await _repository.GetByIdAsync(id);
        if (existingDepartment != null)
        {
            // Generate event ID first
            var eventId = Guid.NewGuid().ToString();
            
            // Store "before" snapshot in Redis BEFORE publishing event
            var beforeSnapshot = AuditEntitySerializer.SerializeDepartment(existingDepartment);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Publish event after Redis key is set
            await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentDeleted, new { DepartmentId = id }, eventId);
        }
        
        await _repository.DeleteAsync(id);
    }
}
