using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly WorkforceDbContext _context;

    public ProjectRepository(WorkforceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.Employee)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.AssignedToEmployee)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        
        // Detach the entity to force fresh reload with navigation properties
        _context.Entry(project).State = EntityState.Detached;
        
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        
        // Detach the entity to force fresh reload
        _context.Entry(project).State = EntityState.Detached;
        
        return project;
    }

    public async Task<Project?> ReloadWithNavigationPropertiesAsync(Guid id)
    {
        // Detach any existing tracked entity with this ID
        var tracked = await _context.Projects.FindAsync(id);
        if (tracked != null)
        {
            _context.Entry(tracked).State = EntityState.Detached;
        }
        
        // Reload fresh from database with all navigation properties
        return await _context.Projects
            .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.Employee)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.AssignedToEmployee)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
