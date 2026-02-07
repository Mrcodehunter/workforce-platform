using WorkforceAPI.Models;

namespace WorkforceAPI.Repositories;

public interface IDesignationRepository
{
    Task<IEnumerable<Designation>> GetAllAsync();
    Task<Designation?> GetByIdAsync(Guid id);
    Task<Designation> CreateAsync(Designation designation);
}
