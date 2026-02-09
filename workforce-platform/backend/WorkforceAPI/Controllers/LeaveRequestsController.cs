using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ILogger<LeaveRequestsController> _logger;

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
    [ProducesResponseType(typeof(IEnumerable<LeaveRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? leaveType = null,
        [FromQuery] Guid? employeeId = null)
    {
        try
        {
            IEnumerable<LeaveRequest> leaveRequests;

            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                leaveRequests = await _leaveRequestService.GetByEmployeeIdAsync(employeeId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(status))
            {
                leaveRequests = await _leaveRequestService.GetByStatusAsync(status);
            }
            else if (!string.IsNullOrWhiteSpace(leaveType))
            {
                leaveRequests = await _leaveRequestService.GetByLeaveTypeAsync(leaveType);
            }
            else
            {
                leaveRequests = await _leaveRequestService.GetAllAsync();
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

            return Ok(leaveRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave requests");
            return StatusCode(500, new { message = "An error occurred while retrieving leave requests" });
        }
    }

    /// <summary>
    /// Get leave request by ID
    /// </summary>
    /// <param name="id">Leave request ID</param>
    /// <returns>Leave request details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeaveRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveRequest>> GetById(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { message = "Leave request ID is required" });
            }

            var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
            if (leaveRequest == null)
            {
                return NotFound(new { message = $"Leave request with ID {id} not found" });
            }
            return Ok(leaveRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave request {LeaveRequestId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the leave request" });
        }
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    /// <param name="leaveRequest">Leave request data</param>
    /// <returns>Created leave request</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LeaveRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(LeaveRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaveRequest>> UpdateStatus(string id, [FromBody] UpdateLeaveRequestStatusRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { message = "Leave request ID is required" });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Status is required" });
            }

            var validStatuses = new[] { "Approved", "Rejected", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Status must be one of: {string.Join(", ", validStatuses)}" });
            }

            // TODO: Get current user from authentication context
            // For now, using a placeholder
            var changedBy = request.ChangedBy ?? "System";

            var updatedLeaveRequest = await _leaveRequestService.UpdateStatusAsync(
                id, 
                request.Status, 
                changedBy, 
                request.Comments);

            return Ok(updatedLeaveRequest);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Leave request not found: {LeaveRequestId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave request status {LeaveRequestId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the leave request status" });
        }
    }

    public class UpdateLeaveRequestStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string? ChangedBy { get; set; }
    }
}
