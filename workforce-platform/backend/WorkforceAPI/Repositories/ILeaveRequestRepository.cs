namespace WorkforceAPI.Repositories;

public interface ILeaveRequestRepository
{
    Task<object?> GetByIdAsync(string id);
    Task<IEnumerable<object>> GetAllAsync();
}
