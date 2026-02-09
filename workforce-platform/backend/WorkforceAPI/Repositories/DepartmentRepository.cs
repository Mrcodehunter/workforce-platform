using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly WorkforceDbContext _context;

    public DepartmentRepository(WorkforceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments.Where(d => !d.IsDeleted).ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        department.Id = Guid.NewGuid();
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var department = await GetByIdAsync(id);
        if (department != null)
        {
            department.IsDeleted = true;
            department.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
