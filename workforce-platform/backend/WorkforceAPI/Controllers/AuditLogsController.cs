using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models.MongoDB;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get all audit logs with optional filtering
    /// </summary>
    /// <param name="entityType">Filter by entity type (Employee, Project, Task, LeaveRequest)</param>
    /// <param name="eventType">Filter by event type (employee.created, project.updated, etc.)</param>
    /// <param name="startDate">Filter by start date (ISO format)</param>
    /// <param name="endDate">Filter by end date (ISO format)</param>
    /// <param name="limit">Limit number of results (default: 100)</param>
    /// <returns>List of audit logs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAll(
        [FromQuery] string? entityType = null,
        [FromQuery] string? eventType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            IEnumerable<AuditLog> auditLogs;

            if (startDate.HasValue && endDate.HasValue)
            {
                auditLogs = await _auditLogService.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else if (!string.IsNullOrWhiteSpace(entityType) && !string.IsNullOrWhiteSpace(eventType))
            {
                // If both filters provided, get by entity type and filter by event type in memory
                auditLogs = await _auditLogService.GetByEntityTypeAsync(entityType);
                auditLogs = auditLogs.Where(log => log.EventType == eventType);
            }
            else if (!string.IsNullOrWhiteSpace(entityType))
            {
                auditLogs = await _auditLogService.GetByEntityTypeAsync(entityType);
            }
            else if (!string.IsNullOrWhiteSpace(eventType))
            {
                auditLogs = await _auditLogService.GetByEventTypeAsync(eventType);
            }
            else if (limit > 0 && limit < 1000)
            {
                auditLogs = await _auditLogService.GetRecentAsync(limit);
            }
            else
            {
                auditLogs = await _auditLogService.GetAllAsync();
            }

            // Apply additional filters if multiple are provided
            if (!string.IsNullOrWhiteSpace(entityType) && auditLogs.Any())
            {
                auditLogs = auditLogs.Where(log => log.EntityType == entityType);
            }
            if (!string.IsNullOrWhiteSpace(eventType) && auditLogs.Any())
            {
                auditLogs = auditLogs.Where(log => log.EventType == eventType);
            }
            if (limit > 0 && limit < 1000 && auditLogs.Count() > limit)
            {
                auditLogs = auditLogs.Take(limit);
            }

            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new { message = "An error occurred while retrieving audit logs" });
        }
    }

    /// <summary>
    /// Get recent audit logs (for dashboard)
    /// </summary>
    /// <param name="limit">Number of recent logs to return (default: 50)</param>
    /// <returns>List of recent audit logs</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetRecent([FromQuery] int limit = 50)
    {
        try
        {
            var auditLogs = await _auditLogService.GetRecentAsync(limit);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent audit logs");
            return StatusCode(500, new { message = "An error occurred while retrieving recent audit logs" });
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    /// <param name="entityType">Entity type (Employee, Project, Task, LeaveRequest)</param>
    /// <param name="entityId">Entity ID</param>
    /// <returns>List of audit logs for the entity</returns>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByEntity(string entityType, string entityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            {
                return BadRequest(new { message = "Entity type and entity ID are required" });
            }

            var auditLogs = await _auditLogService.GetByEntityIdAsync(entityType, entityId);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity {EntityType}/{EntityId}", entityType, entityId);
            return StatusCode(500, new { message = "An error occurred while retrieving audit logs" });
        }
    }

    /// <summary>
    /// Get audit logs by event type
    /// </summary>
    /// <param name="eventType">Event type (e.g., employee.created, project.updated)</param>
    /// <returns>List of audit logs for the event type</returns>
    [HttpGet("event/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByEventType(string eventType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return BadRequest(new { message = "Event type is required" });
            }

            var auditLogs = await _auditLogService.GetByEventTypeAsync(eventType);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for event type {EventType}", eventType);
            return StatusCode(500, new { message = "An error occurred while retrieving audit logs" });
        }
    }
}
