using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository interface for leave request data access operations (MongoDB)
/// </summary>
public interface ILeaveRequestRepository
{
    /// <summary>
    /// Gets leave request by ID
    /// </summary>
    Task<LeaveRequest?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets all leave requests
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    
    /// <summary>
    /// Gets leave requests by employee ID
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
    
    /// <summary>
    /// Gets leave requests by status
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets leave requests by leave type
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(string leaveType);
    
    /// <summary>
    /// Creates a new leave request
    /// </summary>
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    
    /// <summary>
    /// Updates leave request status
    /// </summary>
    Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null);
}
