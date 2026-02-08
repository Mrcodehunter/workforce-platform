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
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    private readonly ILogger<ProjectsController> _logger;
    private readonly IWebHostEnvironment _environment;

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
    [ProducesResponseType(typeof(IEnumerable<ProjectListDto>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid project ID" });
            }

            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
            }

            // Log for debugging
            _logger.LogInformation("Project {ProjectId} has {MemberCount} members", id, project.ProjectMembers?.Count ?? 0);

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the project" });
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="project">Project data</param>
    /// <returns>Created project</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> Update(Guid id, [FromBody] Project project)
    {
        try
        {
            if (id != project.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProject = await _projectService.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
            }

            var updatedProject = await _projectService.UpdateAsync(project);
            return Ok(updatedProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the project" });
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID</param>
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
                return BadRequest(new { message = "Invalid project ID" });
            }

            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
            }

            await _projectService.DeleteAsync(id);
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting project {ProjectId}", id);
            return StatusCode(500, new { message = "A database error occurred while deleting the project" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation deleting project {ProjectId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the project" });
        }
    }

    /// <summary>
    /// Get all tasks for a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>List of tasks</returns>
    [HttpGet("{id}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<TaskListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TaskListDto>>> GetProjectTasks(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid project ID" });
            }

            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
            }

            var tasks = await _taskService.GetTasksByProjectIdAsync(id);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving project tasks" });
        }
    }

    /// <summary>
    /// Create a task for a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="task">Task data</param>
    /// <returns>Created task</returns>
    [HttpPost("{id}/tasks")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> CreateProjectTask(Guid id, [FromBody] TaskItem task)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid project ID" });
            }

            if (id != task.ProjectId)
            {
                return BadRequest(new { message = "Project ID in URL does not match ProjectId in task" });
            }

            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating task for project {ProjectId}", id);
            return StatusCode(500, new { message = "A database error occurred while creating the task" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation creating task for project {ProjectId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while creating the task" });
        }
    }

    /// <summary>
    /// Add a member to a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="request">Member assignment request</param>
    /// <returns>Updated project details</returns>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> AddMember(Guid id, [FromBody] AddProjectMemberRequestDto request)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid project ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _projectService.AddMemberAsync(id, request.EmployeeId, request.Role);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation adding member to project {ProjectId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while adding the member" });
        }
    }

    /// <summary>
    /// Remove a member from a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Updated project details</returns>
    [HttpDelete("{id}/members/{employeeId}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> RemoveMember(Guid id, Guid employeeId)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid project ID" });
            }

            if (employeeId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid employee ID" });
            }

            var project = await _projectService.RemoveMemberAsync(id, employeeId);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation removing member from project {ProjectId}", id);
            
            // Check if it's a "not found" type error
            if (ex.Message.Contains("not found") || ex.Message.Contains("not a member"))
            {
                return NotFound(new { message = ex.Message });
            }
            
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from project {ProjectId}", id);
            return StatusCode(500, new { message = "An error occurred while removing the member" });
        }
    }
}
