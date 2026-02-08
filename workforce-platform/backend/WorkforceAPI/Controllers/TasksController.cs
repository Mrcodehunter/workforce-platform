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
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;
    private readonly IWebHostEnvironment _environment;

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
    [ProducesResponseType(typeof(IEnumerable<TaskListDto>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailDto>> GetById(Guid id)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the task" });
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="task">Task data</param>
    /// <returns>Created task</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> Update(Guid id, [FromBody] TaskItem task)
    {
        try
        {
            if (id != task.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTask = await _taskService.GetByIdAsync(id);
            if (existingTask == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            var updatedTask = await _taskService.UpdateAsync(task);
            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the task" });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid task ID" });
            }

            var task = await _taskService.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            await _taskService.DeleteAsync(id);
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting task {TaskId}", id);
            return StatusCode(500, new { message = "A database error occurred while deleting the task" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation deleting task {TaskId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the task" });
        }
    }

    /// <summary>
    /// Get all tasks assigned to an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>List of tasks</returns>
    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<TaskListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskListDto>>> GetEmployeeTasks(Guid employeeId)
    {
        try
        {
            if (employeeId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid employee ID" });
            }

            var tasks = await _taskService.GetTasksByEmployeeIdAsync(employeeId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for employee {EmployeeId}", employeeId);
            return StatusCode(500, new { message = "An error occurred while retrieving employee tasks" });
        }
    }

    /// <summary>
    /// Update task status
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid task ID" });
            }

            var validStatuses = new[] { "ToDo", "InProgress", "InReview", "Done", "Cancelled" };
            if (request == null || string.IsNullOrWhiteSpace(request.Status) || !validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}" });
            }

            var task = await _taskService.UpdateTaskStatusAsync(id, request.Status);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation updating task status {TaskId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating task status {TaskId}", id);
            return StatusCode(500, new { message = "A database error occurred while updating the task status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the task status" });
        }
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
