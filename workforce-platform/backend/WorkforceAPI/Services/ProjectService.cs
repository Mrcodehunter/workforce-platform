using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.EventPublisher;

namespace WorkforceAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public ProjectService(IProjectRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<ProjectListDto>> GetAllAsync()
    {
        var projects = await _repository.GetAllAsync();
        return projects.Select(p => new ProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            MemberCount = p.ProjectMembers != null ? p.ProjectMembers.Count : 0,
            TaskCount = p.Tasks != null ? p.Tasks.Count : 0
        });
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(Guid id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project == null)
            return null;

        return new ProjectDetailDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            IsDeleted = project.IsDeleted,
            ProjectMembers = project.ProjectMembers?.Select(pm => new ProjectMemberWithEmployeeDto
            {
                ProjectId = pm.ProjectId,
                EmployeeId = pm.EmployeeId,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt,
                Employee = pm.Employee != null ? new EmployeeBasicDto
                {
                    Id = pm.Employee.Id,
                    FirstName = pm.Employee.FirstName,
                    LastName = pm.Employee.LastName,
                    Email = pm.Employee.Email
                } : null
            }).ToList() ?? new List<ProjectMemberWithEmployeeDto>(),
            Tasks = project.Tasks?.Select(t => new TaskItemDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                AssignedToEmployeeId = t.AssignedToEmployeeId,
                AssignedToEmployee = t.AssignedToEmployee != null ? new EmployeeBasicDto
                {
                    Id = t.AssignedToEmployee.Id,
                    FirstName = t.AssignedToEmployee.FirstName,
                    LastName = t.AssignedToEmployee.LastName,
                    Email = t.AssignedToEmployee.Email
                } : null
            }).ToList() ?? new List<TaskItemDto>()
        };
    }

    public async Task<Project> CreateAsync(Project project)
    {
        // Business logic: Set timestamps and IDs
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        project.IsDeleted = false;

        // Clear navigation properties to avoid tracking issues
        project.ProjectMembers = new List<ProjectMember>();
        project.Tasks = new List<TaskItem>();

        var result = await _repository.CreateAsync(project);
        
        // Reload with navigation properties for event
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("project.created", new { ProjectId = result.Id });
        
        return reloaded ?? result;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        var existingProject = await _repository.GetByIdAsync(project.Id);
        if (existingProject == null)
        {
            throw new InvalidOperationException($"Project with ID {project.Id} not found");
        }

        // Update only the properties that should be updated
        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.Status = project.Status;
        existingProject.StartDate = project.StartDate;
        existingProject.EndDate = project.EndDate;
        existingProject.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingProject);
        
        // Reload with navigation properties
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("project.updated", new { ProjectId = result.Id });
        
        return reloaded ?? result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("project.deleted", new { ProjectId = id });
    }
}
