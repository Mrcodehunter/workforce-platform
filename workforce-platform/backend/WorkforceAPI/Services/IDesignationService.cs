using WorkforceAPI.Models;

namespace WorkforceAPI.Services;

public interface IDesignationService
{
    Task<IEnumerable<Designation>> GetAllAsync();
    Task<Designation?> GetByIdAsync(Guid id);
    Task<Designation> CreateAsync(Designation designation);
}
