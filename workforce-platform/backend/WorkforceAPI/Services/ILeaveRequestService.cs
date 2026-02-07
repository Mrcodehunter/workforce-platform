namespace WorkforceAPI.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<object?> GetByIdAsync(string id);
}
