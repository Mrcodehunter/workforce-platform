using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

public interface IProjectService
{
    System.Threading.Tasks.Task<IEnumerable<ProjectListDto>> GetAllAsync();
    System.Threading.Tasks.Task<ProjectDetailDto?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Project> CreateAsync(Project project);
    System.Threading.Tasks.Task<Project> UpdateAsync(Project project);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
