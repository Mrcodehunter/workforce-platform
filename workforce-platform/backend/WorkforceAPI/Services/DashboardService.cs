using WorkforceAPI.Repositories;

namespace WorkforceAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly IReportRepository _reportRepository;

    public DashboardService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<object> GetSummaryAsync()
    {
        var report = await _reportRepository.GetLatestAsync();
        return report ?? new { message = "No reports available" };
    }
}
