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
        return await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var project = await GetByIdAsync(id);
        if (project != null)
        {
            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
