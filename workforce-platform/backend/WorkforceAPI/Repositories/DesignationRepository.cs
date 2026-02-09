using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

/// <summary>
/// Repository implementation for designation data access operations
/// </summary>
public class DesignationRepository : IDesignationRepository
{
    private readonly WorkforceDbContext _context;

    /// <summary>
    /// Initializes a new instance of DesignationRepository
    /// </summary>
    public DesignationRepository(WorkforceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Designation>> GetAllAsync()
    {
        return await _context.Designations.ToListAsync();
    }

    public async Task<Designation?> GetByIdAsync(Guid id)
    {
        return await _context.Designations.FindAsync(id);
    }

    public async Task<Designation> CreateAsync(Designation designation)
    {
        designation.Id = Guid.NewGuid();
        designation.CreatedAt = DateTime.UtcNow;
        designation.UpdatedAt = DateTime.UtcNow;
        _context.Designations.Add(designation);
        await _context.SaveChangesAsync();
        return designation;
    }

    public async Task<Designation> UpdateAsync(Designation designation)
    {
        designation.UpdatedAt = DateTime.UtcNow;
        _context.Designations.Update(designation);
        await _context.SaveChangesAsync();
        return designation;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var designation = await GetByIdAsync(id);
        if (designation != null)
        {
            _context.Designations.Remove(designation);
            await _context.SaveChangesAsync();
        }
    }
}
