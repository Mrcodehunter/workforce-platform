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
    /// Get all leave requests
    /// </summary>
    /// <returns>List of leave requests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequest>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAll()
    {
        try
        {
            var leaveRequests = await _leaveRequestService.GetAllAsync();
            var typedRequests = leaveRequests.Cast<LeaveRequest>();
            return Ok(typedRequests);
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
            var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
            if (leaveRequest == null)
            {
                return NotFound(new { message = $"Leave request with ID {id} not found" });
            }
            return Ok((LeaveRequest)leaveRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave request {LeaveRequestId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the leave request" });
        }
    }
}
