using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

/// <summary>
/// Service implementation for department-related business operations
/// </summary>
/// <remarks>
/// Handles department CRUD operations with audit trail support.
/// </remarks>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    /// <summary>
    /// Initializes a new instance of DepartmentService
    /// </summary>
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
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeDepartment(existingDepartment);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2 & 3: Execute business logic and DB operations
        var result = await _repository.UpdateAsync(department);
        
        // Step 4: Retrieve updated data from DB (result is already the updated entity)
        // Step 5: Save afterSnapshot into Redis
        var afterSnapshot = AuditEntitySerializer.SerializeDepartment(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentUpdated, new { DepartmentId = department.Id }, eventId);
        
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
            
            // Step 1: Save beforeSnapshot into Redis
            var beforeSnapshot = AuditEntitySerializer.SerializeDepartment(existingDepartment);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            await _eventPublisher.PublishEventAsync(AuditEventType.DepartmentDeleted, new { DepartmentId = id }, eventId);
        }
        else
        {
            await _repository.DeleteAsync(id);
        }
    }
}
