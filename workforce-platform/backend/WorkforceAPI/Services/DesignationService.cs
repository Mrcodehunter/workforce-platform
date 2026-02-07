using WorkforceAPI.Models;
using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repository;

    public DesignationService(IDesignationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Designation>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Designation?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Designation> CreateAsync(Designation designation)
    {
        return await _repository.CreateAsync(designation);
    }
}
