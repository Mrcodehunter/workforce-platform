namespace WorkforceAPI.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<object>> GetAllAsync();
}
