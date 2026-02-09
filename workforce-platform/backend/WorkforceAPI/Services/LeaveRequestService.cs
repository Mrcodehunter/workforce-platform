using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using WorkforceAPI.Exceptions;
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

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync(string? status = null, string? leaveType = null, Guid? employeeId = null)
    {
        IEnumerable<LeaveRequest> leaveRequests;

        // Apply filters based on provided parameters
        if (employeeId.HasValue && employeeId.Value != Guid.Empty)
        {
            leaveRequests = await _repository.GetByEmployeeIdAsync(employeeId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            leaveRequests = await _repository.GetByStatusAsync(status);
        }
        else if (!string.IsNullOrWhiteSpace(leaveType))
        {
            leaveRequests = await _repository.GetByLeaveTypeAsync(leaveType);
        }
        else
        {
            leaveRequests = await _repository.GetAllAsync();
        }

        // Apply additional filters if multiple are provided
        if (!string.IsNullOrWhiteSpace(status) && leaveRequests.Any())
        {
            leaveRequests = leaveRequests.Where(lr => lr.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(leaveType) && leaveRequests.Any())
        {
            leaveRequests = leaveRequests.Where(lr => lr.LeaveType == leaveType);
        }
        if (employeeId.HasValue && employeeId.Value != Guid.Empty && leaveRequests.Any())
        {
            leaveRequests = leaveRequests.Where(lr => lr.EmployeeId == employeeId.Value);
        }

        return leaveRequests;
    }

    public async Task<LeaveRequest> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("Leave request ID is required", nameof(id));
        }

        var leaveRequest = await _repository.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new EntityNotFoundException("LeaveRequest", id);
        }

        return leaveRequest;
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
        // Validate input
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("Leave request ID is required", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Status is required", nameof(status));
        }

        // Validate status value
        var validStatuses = new[] { "Approved", "Rejected", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new ValidationException($"Status must be one of: {string.Join(", ", validStatuses)}", nameof(status), status);
        }

        // Get existing leave request for "before" snapshot
        var existingLeaveRequest = await _repository.GetByIdAsync(id);
        if (existingLeaveRequest == null)
        {
            throw new EntityNotFoundException("LeaveRequest", id);
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
