using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard summary data
    /// </summary>
    /// <returns>Dashboard summary including statistics and recent activity</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetSummary()
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard summary" });
        }
    }
}
