using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public interface IProjectRepository
{
    System.Threading.Tasks.Task<IEnumerable<Project>> GetAllAsync();
    System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Project> CreateAsync(Project project);
    System.Threading.Tasks.Task<Project> UpdateAsync(Project project);
    System.Threading.Tasks.Task<Project?> ReloadWithNavigationPropertiesAsync(Guid id);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
    System.Threading.Tasks.Task<bool> IsMemberAsync(Guid projectId, Guid employeeId);
    System.Threading.Tasks.Task<ProjectMember> AddMemberAsync(ProjectMember member);
    System.Threading.Tasks.Task RemoveMemberAsync(Guid projectId, Guid employeeId);
}
