using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public interface IDepartmentRepository
{
    System.Threading.Tasks.Task<IEnumerable<Department>> GetAllAsync();
    System.Threading.Tasks.Task<Department?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Department> CreateAsync(Department department);
    System.Threading.Tasks.Task<Department> UpdateAsync(Department department);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
