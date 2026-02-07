using WorkforceAPI.Models;

namespace WorkforceAPI.Services;

public interface IEmployeeService
{
    System.Threading.Tasks.Task<IEnumerable<Employee>> GetAllAsync();
    System.Threading.Tasks.Task<Employee?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Employee> CreateAsync(Employee employee);
    System.Threading.Tasks.Task<Employee> UpdateAsync(Employee employee);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
