using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Get all employees (non-paginated - for backward compatibility)
    /// </summary>
    /// <returns>List of employees</returns>
    [HttpGet("all")]
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
    /// Get paginated employees
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of employees</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeListDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _employeeService.GetPagedAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated employees");
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDetailDto>> GetById(Guid id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return Ok(employee);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="employee">Employee data</param>
    /// <returns>Created employee</returns>
    [HttpPost]
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
    public async Task<ActionResult<Employee>> Update(Guid id, [FromBody] Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in body" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedEmployee = await _employeeService.UpdateAsync(employee);
        return Ok(updatedEmployee);
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(id);
        return NoContent();
    }
}
