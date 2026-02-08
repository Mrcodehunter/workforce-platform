using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Repositories;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public LeaveRequestService(ILeaveRequestRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
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
        
        // Publish event
        await _eventPublisher.PublishEventAsync(AuditEventType.LeaveRequestCreated, new 
        { 
            LeaveRequestId = result.Id,
            EmployeeId = result.EmployeeId,
            LeaveType = result.LeaveType,
            StartDate = result.StartDate,
            EndDate = result.EndDate
        });
        
        return result;
    }

    public async Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null)
    {
        var result = await _repository.UpdateStatusAsync(id, status, changedBy, comments);
        
        // Publish appropriate event based on status
        var eventType = status switch
        {
            "Approved" => AuditEventType.LeaveRequestApproved,
            "Rejected" => AuditEventType.LeaveRequestRejected,
            "Cancelled" => AuditEventType.LeaveRequestCancelled,
            _ => AuditEventType.LeaveRequestUpdated
        };
        
        await _eventPublisher.PublishEventAsync(eventType, new 
        { 
            LeaveRequestId = result.Id,
            EmployeeId = result.EmployeeId,
            Status = status,
            ChangedBy = changedBy,
            Comments = comments
        });
        
        return result;
    }
}
