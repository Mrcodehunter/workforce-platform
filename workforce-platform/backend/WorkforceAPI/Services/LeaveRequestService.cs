using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;

    public LeaveRequestService(ILeaveRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<object?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
