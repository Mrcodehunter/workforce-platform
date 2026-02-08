using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DesignationsController : ControllerBase
{
    private readonly IDesignationService _designationService;
    private readonly ILogger<DesignationsController> _logger;

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
    [ProducesResponseType(typeof(IEnumerable<Designation>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(Designation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Designation>> GetById(Guid id)
    {
        try
        {
            var designation = await _designationService.GetByIdAsync(id);
            if (designation == null)
            {
                return NotFound(new { message = $"Designation with ID {id} not found" });
            }
            return Ok(designation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving designation {DesignationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the designation" });
        }
    }

    /// <summary>
    /// Create a new designation
    /// </summary>
    /// <param name="designation">Designation data</param>
    /// <returns>Created designation</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Designation), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(Designation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Designation>> Update(Guid id, [FromBody] Designation designation)
    {
        try
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Designation {DesignationId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating designation {DesignationId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the designation" });
        }
    }

    /// <summary>
    /// Delete a designation
    /// </summary>
    /// <param name="id">Designation ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
