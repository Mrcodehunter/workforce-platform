using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly WorkforceDbContext _context;

    public TaskRepository(WorkforceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.Where(t => !t.IsDeleted).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.Id = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task != null)
        {
            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
