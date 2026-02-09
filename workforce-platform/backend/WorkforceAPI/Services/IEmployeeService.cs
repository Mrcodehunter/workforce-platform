using WorkforceAPI.Models;
using WorkforceAPI.Models.DTOs;

namespace WorkforceAPI.Services;

public interface IEmployeeService
{
    System.Threading.Tasks.Task<IEnumerable<EmployeeListDto>> GetAllAsync();
    System.Threading.Tasks.Task<PagedResult<EmployeeListDto>> GetPagedAsync(int page, int pageSize);
    System.Threading.Tasks.Task<EmployeeDetailDto> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Employee> CreateAsync(Employee employee);
    System.Threading.Tasks.Task<Employee> UpdateAsync(Employee employee);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
