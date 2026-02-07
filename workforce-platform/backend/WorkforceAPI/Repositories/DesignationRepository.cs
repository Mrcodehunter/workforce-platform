using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public class DesignationRepository : IDesignationRepository
{
    private readonly WorkforceDbContext _context;

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
}
