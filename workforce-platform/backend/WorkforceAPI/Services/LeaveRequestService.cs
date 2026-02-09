using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    public LeaveRequestService(ILeaveRequestRepository repository, IRabbitMqPublisher eventPublisher, IRedisCache redisCache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<LeaveRequest?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _repository.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(string leaveType)
    {
        return await _repository.GetByLeaveTypeAsync(leaveType);
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
    {
        // Business logic: Ensure status is Pending for new requests
        leaveRequest.Status = "Pending";
        
        var result = await _repository.CreateAsync(leaveRequest);
        
        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        var afterSnapshot = AuditEntitySerializer.SerializeLeaveRequest(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.LeaveRequestCreated, new 
        { 
            LeaveRequestId = result.Id,
            EmployeeId = result.EmployeeId,
            LeaveType = result.LeaveType,
            StartDate = result.StartDate,
            EndDate = result.EndDate
        }, eventId);
        
        return result;
    }

    public async Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null)
    {
        // Get existing leave request for "before" snapshot
        var existingLeaveRequest = await _repository.GetByIdAsync(id);
        if (existingLeaveRequest == null)
        {
            throw new InvalidOperationException($"Leave request with ID {id} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Capture "before" snapshot and store in Redis BEFORE publishing event
        var beforeSnapshot = AuditEntitySerializer.SerializeLeaveRequest(existingLeaveRequest);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
        
        var result = await _repository.UpdateStatusAsync(id, status, changedBy, comments);
        
        // Capture "after" snapshot
        var afterSnapshot = AuditEntitySerializer.SerializeLeaveRequest(result);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Publish appropriate event based on status
        var eventType = status switch
        {
            "Approved" => AuditEventType.LeaveRequestApproved,
            "Rejected" => AuditEventType.LeaveRequestRejected,
            "Cancelled" => AuditEventType.LeaveRequestCancelled,
            _ => AuditEventType.LeaveRequestUpdated
        };
        
        // Publish event after Redis keys are set
        await _eventPublisher.PublishEventAsync(eventType, new 
        { 
            LeaveRequestId = result.Id,
            EmployeeId = result.EmployeeId,
            Status = status,
            ChangedBy = changedBy,
            Comments = comments
        }, eventId);
        
        return result;
    }
}
