namespace WorkforceAPI.Repositories;

public interface IReportRepository
{
    Task<object?> GetLatestAsync();
}
