using Microsoft.AspNetCore.Mvc;
using WorkforceAPI.Models;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<Department>> GetById(Guid id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound(new { message = $"Department with ID {id} not found" });
        }
        return Ok(department);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <param name="department">Department data</param>
    /// <returns>Created department</returns>
    [HttpPost]
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
    public async Task<ActionResult<Department>> Update(Guid id, [FromBody] Department department)
    {
        if (id != department.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in body" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedDepartment = await _departmentService.UpdateAsync(department);
        return Ok(updatedDepartment);
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}
