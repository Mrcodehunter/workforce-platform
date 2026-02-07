namespace WorkforceAPI.Services;

public interface IDashboardService
{
    Task<object> GetSummaryAsync();
}
