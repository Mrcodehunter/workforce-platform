using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    /// <returns>List of departments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Department>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Department>>> GetAll()
    {
        try
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, new { message = "An error occurred while retrieving departments" });
        }
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Department details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Department>> GetById(Guid id)
    {
        try
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }
            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the department" });
        }
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <param name="department">Department data</param>
    /// <returns>Created department</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Department), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Department>> Create([FromBody] Department department)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDepartment = await _departmentService.CreateAsync(department);
            return CreatedAtAction(nameof(GetById), new { id = createdDepartment.Id }, createdDepartment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, new { message = "An error occurred while creating the department" });
        }
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="department">Updated department data</param>
    /// <returns>Updated department</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Department>> Update(Guid id, [FromBody] Department department)
    {
        try
        {
            if (id != department.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingDepartment = await _departmentService.GetByIdAsync(id);
            if (existingDepartment == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            var updatedDepartment = await _departmentService.UpdateAsync(department);
            return Ok(updatedDepartment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the department" });
        }
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            await _departmentService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the department" });
        }
    }
}
