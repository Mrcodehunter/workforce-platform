using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmployeesController(
        IEmployeeService employeeService, 
        ILogger<EmployeesController> logger,
        IWebHostEnvironment environment)
    {
        _employeeService = employeeService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Get all employees
    /// </summary>
    /// <returns>List of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAll()
    {
        try
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDetailDto>> GetById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid employee ID" });
            }

            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
                return NotFound(new { message = $"Employee with ID {id} not found" });
            }

            return Ok(employee);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error retrieving employee {EmployeeId}", id);
            return StatusCode(500, new { 
                message = "A database error occurred while retrieving the employee",
                error = _environment.IsDevelopment() ? ex.Message : null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving employee {EmployeeId}", id);
            return StatusCode(500, new { 
                message = "An error occurred while retrieving the employee",
                error = _environment.IsDevelopment() ? ex.Message : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving employee {EmployeeId}: {Message}", id, ex.Message);
            return StatusCode(500, new { 
                message = "An unexpected error occurred while retrieving the employee",
                error = _environment.IsDevelopment() ? ex.Message : null
            });
        }
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="employee">Employee data</param>
    /// <returns>Created employee</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Employee>> Create([FromBody] Employee employee)
    {
        try
        {
            // FluentValidation automatically validates and populates ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var createdEmployee = await _employeeService.CreateAsync(employee);
            return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating employee");
            if (ex.InnerException?.Message.Contains("duplicate") == true || 
                ex.InnerException?.Message.Contains("unique") == true)
            {
                return BadRequest(new { message = "An employee with this email already exists" });
            }
            return StatusCode(500, new { message = "An error occurred while creating the employee" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {Message}", ex.Message);
            return StatusCode(500, new { message = $"An error occurred while creating the employee: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="employee">Updated employee data</param>
    /// <returns>Updated employee</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Employee>> Update(Guid id, [FromBody] Employee employee)
    {
        try
        {
            if (id != employee.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body" });
            }

            // FluentValidation automatically validates and populates ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var existingEmployee = await _employeeService.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} not found" });
            }

            var updatedEmployee = await _employeeService.UpdateAsync(employee);
            return Ok(updatedEmployee);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating employee {EmployeeId}", id);
            if (ex.InnerException?.Message.Contains("duplicate") == true || 
                ex.InnerException?.Message.Contains("unique") == true)
            {
                return BadRequest(new { message = "An employee with this email already exists" });
            }
            return StatusCode(500, new { message = "An error occurred while updating the employee" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}: {Message}", id, ex.Message);
            return StatusCode(500, new { message = $"An error occurred while updating the employee: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} not found" });
            }

            await _employeeService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the employee" });
        }
    }
}
