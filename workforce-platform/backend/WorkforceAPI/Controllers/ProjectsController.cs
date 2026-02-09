using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Services;

namespace WorkforceAPI.Controllers;

/// <summary>
/// API controller for project management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    private readonly ILogger<ProjectsController> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of ProjectsController
    /// </summary>
    public ProjectsController(
        IProjectService projectService,
        ITaskService taskService,
        ILogger<ProjectsController> logger,
        IWebHostEnvironment environment)
    {
        _projectService = projectService;
        _taskService = taskService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    /// <returns>List of projects</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectListDto>>> GetAll()
    {
        try
        {
            var projects = await _projectService.GetAllAsync();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, new { message = "An error occurred while retrieving projects" });
        }
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound(new { message = $"Project with ID {id} not found" });
        }
        return Ok(project);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="project">Project data</param>
    /// <returns>Created project</returns>
    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] Project project)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProject = await _projectService.CreateAsync(project);
            return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, new { message = "An error occurred while creating the project" });
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="project">Updated project data</param>
    /// <returns>Updated project</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Project>> Update(Guid id, [FromBody] Project project)
    {
        if (id != project.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in body" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProject = await _projectService.UpdateAsync(project);
        return Ok(updatedProject);
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _projectService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get all tasks for a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of tasks</returns>
    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskListDto>>> GetProjectTasks(Guid id)
    {
        var tasks = await _taskService.GetTasksByProjectIdAsync(id);
        return Ok(tasks);
    }

    /// <summary>
    /// Create a task for a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="task">Task data</param>
    /// <returns>Created task</returns>
    [HttpPost("{id}/tasks")]
    public async Task<ActionResult<TaskItem>> CreateProjectTask(Guid id, [FromBody] TaskItem task)
    {
        if (id != task.ProjectId)
        {
            return BadRequest(new { message = "Project ID in URL does not match ProjectId in task" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdTask = await _taskService.CreateAsync(task);
        return CreatedAtAction(
            nameof(TasksController.GetById),
            "Tasks",
            new { id = createdTask.Id },
            createdTask);
    }

    /// <summary>
    /// Add a member to a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="request">Member assignment request</param>
    /// <returns>Updated project details</returns>
    [HttpPost("{id}/members")]
    public async Task<ActionResult<ProjectDetailDto>> AddMember(Guid id, [FromBody] AddProjectMemberRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var project = await _projectService.AddMemberAsync(id, request.EmployeeId, request.Role);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    /// <summary>
    /// Remove a member from a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Updated project details</returns>
    [HttpDelete("{id}/members/{employeeId}")]
    public async Task<ActionResult<ProjectDetailDto>> RemoveMember(Guid id, Guid employeeId)
    {
        var project = await _projectService.RemoveMemberAsync(id, employeeId);
        return Ok(project);
    }
}
