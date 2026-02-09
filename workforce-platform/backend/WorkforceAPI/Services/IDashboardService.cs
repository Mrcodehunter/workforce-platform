namespace WorkforceAPI.Services;

/// <summary>
/// Service interface for dashboard summary operations
/// </summary>
/// <remarks>
/// Provides aggregated statistics and summary data for the dashboard view.
/// </remarks>
public interface IDashboardService
{
    /// <summary>
    /// Retrieves dashboard summary with aggregated statistics
    /// </summary>
    Task<object> GetSummaryAsync();
}
