using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for leave request-related business operations
/// </summary>
/// <remarks>
/// Handles leave request CRUD operations and status updates with audit trail support.
/// Leave requests are stored in MongoDB.
/// </remarks>
public interface ILeaveRequestService
{
    /// <summary>
    /// Retrieves all leave requests with optional filtering
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetAllAsync(string? status = null, string? leaveType = null, Guid? employeeId = null);
    
    /// <summary>
    /// Retrieves a leave request by ID
    /// </summary>
    Task<LeaveRequest> GetByIdAsync(string id);
    
    /// <summary>
    /// Retrieves leave requests for a specific employee
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId);
    
    /// <summary>
    /// Retrieves leave requests by status
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Retrieves leave requests by leave type
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(string leaveType);
    
    /// <summary>
    /// Creates a new leave request
    /// </summary>
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
    
    /// <summary>
    /// Updates the status of a leave request (approval workflow)
    /// </summary>
    Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null);
}
