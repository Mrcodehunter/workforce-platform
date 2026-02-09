using WorkforceAPI.Models;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

/// <summary>
/// Service implementation for designation-related business operations
/// </summary>
/// <remarks>
/// Handles designation CRUD operations with audit trail support.
/// </remarks>
public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    /// <summary>
    /// Initializes a new instance of DesignationService
    /// </summary>
    public DesignationService(IDesignationRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
    }

    public async Task<IEnumerable<Designation>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Designation?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Designation> CreateAsync(Designation designation)
    {
        var result = await _repository.CreateAsync(designation);
        
        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        var afterSnapshot = AuditEntitySerializer.SerializeDesignation(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.DesignationCreated, new { DesignationId = result.Id }, eventId);
        
        return result;
    }

    public async Task<Designation> UpdateAsync(Designation designation)
    {
        // Get existing designation for "before" snapshot
        var existingDesignation = await _repository.GetByIdAsync(designation.Id);
        if (existingDesignation == null)
        {
            throw new InvalidOperationException($"Designation with ID {designation.Id} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeDesignation(existingDesignation);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2 & 3: Execute business logic and DB operations
        var result = await _repository.UpdateAsync(designation);
        
        // Step 4: Retrieve updated data from DB (result is already the updated entity)
        // Step 5: Save afterSnapshot into Redis
        var afterSnapshot = AuditEntitySerializer.SerializeDesignation(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.DesignationUpdated, new { DesignationId = designation.Id }, eventId);
        
        return result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        var existingDesignation = await _repository.GetByIdAsync(id);
        if (existingDesignation != null)
        {
            // Generate event ID first
            var eventId = Guid.NewGuid().ToString();
            
            // Step 1: Save beforeSnapshot into Redis
            var beforeSnapshot = AuditEntitySerializer.SerializeDesignation(existingDesignation);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            await _eventPublisher.PublishEventAsync(AuditEventType.DesignationDeleted, new { DesignationId = id }, eventId);
        }
        else
        {
            await _repository.DeleteAsync(id);
        }
    }
}
