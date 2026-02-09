using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

/// <summary>
/// API controller for designation management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DesignationsController : ControllerBase
{
    private readonly IDesignationService _designationService;
    private readonly ILogger<DesignationsController> _logger;

    /// <summary>
    /// Initializes a new instance of DesignationsController
    /// </summary>
    public DesignationsController(IDesignationService designationService, ILogger<DesignationsController> logger)
    {
        _designationService = designationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all designations
    /// </summary>
    /// <returns>List of designations</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Designation>>> GetAll()
    {
        try
        {
            var designations = await _designationService.GetAllAsync();
            return Ok(designations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving designations");
            return StatusCode(500, new { message = "An error occurred while retrieving designations" });
        }
    }

    /// <summary>
    /// Get designation by ID
    /// </summary>
    /// <param name="id">Designation ID</param>
    /// <returns>Designation details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Designation>> GetById(Guid id)
    {
        var designation = await _designationService.GetByIdAsync(id);
        if (designation == null)
        {
            return NotFound(new { message = $"Designation with ID {id} not found" });
        }
        return Ok(designation);
    }

    /// <summary>
    /// Create a new designation
    /// </summary>
    /// <param name="designation">Designation data</param>
    /// <returns>Created designation</returns>
    [HttpPost]
    public async Task<ActionResult<Designation>> Create([FromBody] Designation designation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDesignation = await _designationService.CreateAsync(designation);
            return CreatedAtAction(nameof(GetById), new { id = createdDesignation.Id }, createdDesignation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating designation");
            return StatusCode(500, new { message = "An error occurred while creating the designation" });
        }
    }

    /// <summary>
    /// Update an existing designation
    /// </summary>
    /// <param name="id">Designation ID</param>
    /// <param name="designation">Updated designation data</param>
    /// <returns>Updated designation</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Designation>> Update(Guid id, [FromBody] Designation designation)
    {
        if (id != designation.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedDesignation = await _designationService.UpdateAsync(designation);
        return Ok(updatedDesignation);
    }

    /// <summary>
    /// Delete a designation
    /// </summary>
    /// <param name="id">Designation ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _designationService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting designation {DesignationId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the designation" });
        }
    }
}
