using System.Text.Json;
using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.Helpers;
using Workforce.Shared.Cache;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Events;

namespace WorkforceAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRabbitMqPublisher _eventPublisher;
    private readonly IRedisCache _redisCache;

    public ProjectService(
        IProjectRepository repository, 
        IEmployeeRepository employeeRepository,
        IRabbitMqPublisher eventPublisher, 
        IRedisCache redisCache)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _eventPublisher = eventPublisher;
        _redisCache = redisCache;
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
        
        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Reload with navigation properties for event using fresh query
        var reloaded = await _repository.ReloadWithNavigationPropertiesAsync(result.Id);
        
        // Capture "after" snapshot and store in Redis BEFORE publishing event
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeProject(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Publish event after Redis key is set
        await _eventPublisher.PublishEventAsync(AuditEventType.ProjectCreated, new { ProjectId = result.Id }, eventId);
        
        return reloaded ?? result;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        var existingProject = await _repository.GetByIdAsync(project.Id);
        if (existingProject == null)
        {
            throw new InvalidOperationException($"Project with ID {project.Id} not found");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeProject(existingProject);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2: Execute business logic - Update only the properties that should be updated
        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.Status = project.Status;
        existingProject.StartDate = project.StartDate;
        existingProject.EndDate = project.EndDate;
        existingProject.UpdatedAt = DateTime.UtcNow;

        // Step 3: Execute DB operations
        var result = await _repository.UpdateAsync(existingProject);
        
        // Step 4: Retrieve updated data from DB
        // Use ReloadWithNavigationPropertiesAsync to get fresh data from database
        var reloaded = await _repository.ReloadWithNavigationPropertiesAsync(result.Id);
        
        // Step 5: Save afterSnapshot into Redis
        if (reloaded != null)
        {
            var afterSnapshot = AuditEntitySerializer.SerializeProject(reloaded);
            await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        }
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.ProjectUpdated, new { ProjectId = project.Id }, eventId);
        
        return reloaded ?? result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        // Capture "before" snapshot before deletion
        var existingProject = await _repository.GetByIdAsync(id);
        if (existingProject != null)
        {
            // Generate event ID first
            var eventId = Guid.NewGuid().ToString();
            
            // Step 1: Save beforeSnapshot into Redis
            var beforeSnapshot = AuditEntitySerializer.SerializeProject(existingProject);
            await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));
            
            // Step 2 & 3: Execute business logic and DB operations
            await _repository.DeleteAsync(id);
            
            // Step 6: Publish the event (after all operations completed)
            await _eventPublisher.PublishEventAsync(AuditEventType.ProjectDeleted, new { ProjectId = id }, eventId);
        }
        else
        {
            await _repository.DeleteAsync(id);
        }
    }

    public async Task<ProjectDetailDto> AddMemberAsync(Guid projectId, Guid employeeId, string? role)
    {
        // Check if project exists
        var project = await _repository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found");
        }

        // Check if employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID {employeeId} not found");
        }

        // Check if employee is already a member
        var isMember = await _repository.IsMemberAsync(projectId, employeeId);
        if (isMember)
        {
            throw new InvalidOperationException($"Employee {employee.FirstName} {employee.LastName} is already a member of this project");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeProject(project);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2: Execute business logic - Create project member
        var projectMember = new ProjectMember
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        // Step 3: Execute DB operations
        await _repository.AddMemberAsync(projectMember);

        // Step 4: Retrieve updated data from DB
        var updatedProject = await _repository.ReloadWithNavigationPropertiesAsync(projectId);
        if (updatedProject == null)
        {
            throw new InvalidOperationException($"Failed to reload project {projectId} after adding member");
        }
        
        // Step 5: Save afterSnapshot into Redis
        var afterSnapshot = AuditEntitySerializer.SerializeProject(updatedProject);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.ProjectMemberAdded, new { ProjectId = projectId, EmployeeId = employeeId, Role = role }, eventId);

        // Convert to DTO
        return new ProjectDetailDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            Status = updatedProject.Status,
            StartDate = updatedProject.StartDate,
            EndDate = updatedProject.EndDate,
            CreatedAt = updatedProject.CreatedAt,
            UpdatedAt = updatedProject.UpdatedAt,
            IsDeleted = updatedProject.IsDeleted,
            ProjectMembers = updatedProject.ProjectMembers?.Select(pm => new ProjectMemberWithEmployeeDto
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
            Tasks = updatedProject.Tasks?.Select(t => new TaskItemDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                AssignedToEmployeeId = t.AssignedToEmployeeId,
                Priority = t.Priority,
                DueDate = t.DueDate,
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

    public async Task<ProjectDetailDto> RemoveMemberAsync(Guid projectId, Guid employeeId)
    {
        // Check if project exists
        var project = await _repository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found");
        }

        // Check if member exists
        var isMember = await _repository.IsMemberAsync(projectId, employeeId);
        if (!isMember)
        {
            throw new InvalidOperationException($"Employee with ID {employeeId} is not a member of this project");
        }

        // Generate event ID first
        var eventId = Guid.NewGuid().ToString();
        
        // Step 1: Save beforeSnapshot into Redis
        var beforeSnapshot = AuditEntitySerializer.SerializeProject(project);
        await _redisCache.SetAsync($"audit:{eventId}:before", beforeSnapshot, TimeSpan.FromHours(1));

        // Step 2 & 3: Execute business logic and DB operations - Remove member
        await _repository.RemoveMemberAsync(projectId, employeeId);

        // Step 4: Retrieve updated data from DB
        var updatedProject = await _repository.ReloadWithNavigationPropertiesAsync(projectId);
        if (updatedProject == null)
        {
            throw new InvalidOperationException($"Failed to reload project {projectId} after removing member");
        }
        
        // Step 5: Save afterSnapshot into Redis
        var afterSnapshot = AuditEntitySerializer.SerializeProject(updatedProject);
        await _redisCache.SetAsync($"audit:{eventId}:after", afterSnapshot, TimeSpan.FromHours(1));
        
        // Step 6: Publish the event (after all operations completed)
        await _eventPublisher.PublishEventAsync(AuditEventType.ProjectMemberRemoved, new { ProjectId = projectId, EmployeeId = employeeId }, eventId);

        // Convert to DTO
        return new ProjectDetailDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            Status = updatedProject.Status,
            StartDate = updatedProject.StartDate,
            EndDate = updatedProject.EndDate,
            CreatedAt = updatedProject.CreatedAt,
            UpdatedAt = updatedProject.UpdatedAt,
            IsDeleted = updatedProject.IsDeleted,
            ProjectMembers = updatedProject.ProjectMembers?.Select(pm => new ProjectMemberWithEmployeeDto
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
            Tasks = updatedProject.Tasks?.Select(t => new TaskItemDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                AssignedToEmployeeId = t.AssignedToEmployeeId,
                Priority = t.Priority,
                DueDate = t.DueDate,
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
}
