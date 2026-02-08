using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly WorkforceDbContext _context;

    public EmployeeRepository(WorkforceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ProjectMembers)
                .ThenInclude(pm => pm.Project)
            .AsSplitQuery() // Use split query to avoid cartesian explosion
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (employee != null)
        {
            // Break circular reference by clearing ProjectMembers from each Project
            foreach (var projectMember in employee.ProjectMembers)
            {
                if (projectMember.Project != null)
                {
                    projectMember.Project.ProjectMembers = new List<ProjectMember>();
                    projectMember.Project.Tasks = new List<TaskItem>();
                }
            }
        }

        return employee;
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        // Clear navigation properties to avoid tracking issues
        employee.Department = null;
        employee.Designation = null;
        employee.ProjectMembers = new List<ProjectMember>();
        employee.AssignedTasks = new List<TaskItem>();

        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.IsDeleted = false;
        
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await GetByIdAsync(employee.Id) ?? employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employee.Id && !e.IsDeleted);
        
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

        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await GetByIdAsync(employee.Id) ?? existingEmployee;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var employee = await GetByIdAsync(id);
        if (employee != null)
        {
            employee.IsDeleted = true;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
