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
        return await _context.Employees.Where(e => !e.IsDeleted).ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
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
