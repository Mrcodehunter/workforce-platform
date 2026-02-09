using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

/// <summary>
/// API controller for task management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of TasksController
    /// </summary>
    public TasksController(
        ITaskService taskService,
        ILogger<TasksController> logger,
        IWebHostEnvironment environment)
    {
        _taskService = taskService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <returns>List of tasks</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskListDto>>> GetAll()
    {
        try
        {
            var tasks = await _taskService.GetAllAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetailDto>> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound(new { message = $"Task with ID {id} not found" });
        }
        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="task">Task data</param>
    /// <returns>Created task</returns>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem task)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTask = await _taskService.CreateAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "An error occurred while creating the task" });
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="task">Updated task data</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> Update(Guid id, [FromBody] TaskItem task)
    {
        if (id != task.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in body" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedTask = await _taskService.UpdateAsync(task);
        return Ok(updatedTask);
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get all tasks assigned to an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>List of tasks</returns>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<TaskListDto>>> GetEmployeeTasks(Guid employeeId)
    {
        var tasks = await _taskService.GetTasksByEmployeeIdAsync(employeeId);
        return Ok(tasks);
    }

    /// <summary>
    /// Update task status
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskItem>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var task = await _taskService.UpdateTaskStatusAsync(id, request?.Status ?? string.Empty);
        return Ok(task);
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
