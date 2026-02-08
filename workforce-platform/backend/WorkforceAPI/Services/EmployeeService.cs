using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;
using WorkforceAPI.Repositories;
using WorkforceAPI.EventPublisher;

namespace WorkforceAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IRabbitMqPublisher _eventPublisher;

    public EmployeeService(IEmployeeRepository repository, IRabbitMqPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<EmployeeListDto>> GetAllAsync()
    {
        var employees = await _repository.GetAllAsync();
        return employees.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            IsActive = e.IsActive,
            Phone = e.Phone,
            AvatarUrl = e.AvatarUrl,
            Department = e.Department != null ? new DepartmentDto
            {
                Id = e.Department.Id,
                Name = e.Department.Name,
                Description = e.Department.Description
            } : null,
            Designation = e.Designation != null ? new DesignationDto
            {
                Id = e.Designation.Id,
                Title = e.Designation.Title,
                Level = e.Designation.Level,
                Description = e.Designation.Description
            } : null
        });
    }

    public async Task<EmployeeDetailDto?> GetByIdAsync(Guid id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            return null;

        return new EmployeeDetailDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            Salary = employee.Salary,
            JoiningDate = employee.JoiningDate,
            Phone = employee.Phone,
            Address = employee.Address,
            City = employee.City,
            Country = employee.Country,
            Skills = employee.Skills ?? new List<string>(),
            AvatarUrl = employee.AvatarUrl,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            IsDeleted = employee.IsDeleted,
            Department = employee.Department != null ? new DepartmentDto
            {
                Id = employee.Department.Id,
                Name = employee.Department.Name,
                Description = employee.Department.Description
            } : null,
            Designation = employee.Designation != null ? new DesignationDto
            {
                Id = employee.Designation.Id,
                Title = employee.Designation.Title,
                Level = employee.Designation.Level,
                Description = employee.Designation.Description
            } : null,
            ProjectMembers = employee.ProjectMembers?.Select(pm => new ProjectMemberDto
            {
                ProjectId = pm.ProjectId,
                EmployeeId = pm.EmployeeId,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt,
                Project = pm.Project != null ? new ProjectSummaryDto
                {
                    Id = pm.Project.Id,
                    Name = pm.Project.Name,
                    Description = pm.Project.Description,
                    Status = pm.Project.Status,
                    StartDate = pm.Project.StartDate,
                    EndDate = pm.Project.EndDate
                } : null
            }).ToList() ?? new List<ProjectMemberDto>()
        };
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        // Business logic: Set timestamps and IDs
        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.IsDeleted = false;

        // Clear navigation properties to avoid tracking issues
        employee.Department = null;
        employee.Designation = null;
        employee.ProjectMembers = new List<ProjectMember>();
        employee.AssignedTasks = new List<TaskItem>();

        var result = await _repository.CreateAsync(employee);
        
        // Reload with navigation properties for event
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("employee.created", new { EmployeeId = result.Id });
        
        return reloaded ?? result;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        var existingEmployee = await _repository.GetByIdAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new InvalidOperationException($"Employee with ID {employee.Id} not found");
        }

        // Update only the properties that should be updated
        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Phone = employee.Phone;
        existingEmployee.Address = employee.Address;
        existingEmployee.City = employee.City;
        existingEmployee.Country = employee.Country;
        existingEmployee.Salary = employee.Salary;
        existingEmployee.JoiningDate = employee.JoiningDate;
        existingEmployee.DepartmentId = employee.DepartmentId;
        existingEmployee.DesignationId = employee.DesignationId;
        existingEmployee.Skills = employee.Skills;
        existingEmployee.AvatarUrl = employee.AvatarUrl;
        existingEmployee.IsActive = employee.IsActive;
        existingEmployee.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingEmployee);
        
        // Reload with navigation properties
        var reloaded = await _repository.GetByIdAsync(result.Id);
        await _eventPublisher.PublishEventAsync("employee.updated", new { EmployeeId = result.Id });
        
        return reloaded ?? result;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _eventPublisher.PublishEventAsync("employee.deleted", new { EmployeeId = id });
    }
}
