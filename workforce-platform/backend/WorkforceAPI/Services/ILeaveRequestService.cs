using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<LeaveRequest?> GetByIdAsync(string id);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(string leaveType);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null);
}
