using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository implementation for employee data access operations
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly WorkforceDbContext _context;

    /// <summary>
    /// Initializes a new instance of EmployeeRepository
    /// </summary>
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

    public async Task<(IEnumerable<Employee> Data, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => !e.IsDeleted);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ProjectMembers)
                .ThenInclude(pm => pm.Project)
            .Include(e => e.AssignedTasks)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            employee.IsDeleted = true;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
