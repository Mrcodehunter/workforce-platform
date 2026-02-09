using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

/// <summary>
/// API controller for leave request management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ILogger<LeaveRequestsController> _logger;

    /// <summary>
    /// Initializes a new instance of LeaveRequestsController
    /// </summary>
    public LeaveRequestsController(ILeaveRequestService leaveRequestService, ILogger<LeaveRequestsController> logger)
    {
        _leaveRequestService = leaveRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all leave requests with optional filtering
    /// </summary>
    /// <param name="status">Filter by status (Pending, Approved, Rejected, Cancelled)</param>
    /// <param name="leaveType">Filter by leave type (Sick, Casual, Annual, Unpaid)</param>
    /// <param name="employeeId">Filter by employee ID</param>
    /// <returns>List of leave requests</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? leaveType = null,
        [FromQuery] Guid? employeeId = null)
    {
        var leaveRequests = await _leaveRequestService.GetAllAsync(status, leaveType, employeeId);
        return Ok(leaveRequests);
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    /// <param name="id">Leave request ID</param>
    /// <returns>Leave request details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequest>> GetById(string id)
    {
        var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
        return Ok(leaveRequest);
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    /// <param name="leaveRequest">Leave request data</param>
    /// <returns>Created leave request</returns>
    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> Create([FromBody] LeaveRequest leaveRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdLeaveRequest = await _leaveRequestService.CreateAsync(leaveRequest);
            return CreatedAtAction(nameof(GetById), new { id = createdLeaveRequest.Id }, createdLeaveRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            return StatusCode(500, new { message = "An error occurred while creating the leave request" });
        }
    }

    /// <summary>
    /// Update leave request status (approve/reject/cancel)
    /// </summary>
    /// <param name="id">Leave request ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated leave request</returns>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<LeaveRequest>> UpdateStatus(string id, [FromBody] UpdateLeaveRequestStatusRequest request)
    {
        // TODO: Get current user from authentication context
        // For now, using a placeholder
        var changedBy = request?.ChangedBy ?? "System";

        var updatedLeaveRequest = await _leaveRequestService.UpdateStatusAsync(
            id,
            request?.Status ?? string.Empty,
            changedBy,
            request?.Comments);

        return Ok(updatedLeaveRequest);
    }

    public class UpdateLeaveRequestStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string? ChangedBy { get; set; }
    }
}
