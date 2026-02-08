using WorkerService.AuditLogger.Services;
using Microsoft.Extensions.Logging;

namespace WorkerService.AuditLogger;

public class AuditWorker : BackgroundService
{
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly ILogger<AuditWorker> _logger;

    public AuditWorker(IRabbitMqConsumer rabbitMqConsumer, ILogger<AuditWorker> logger)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Worker starting...");
        
        try
        {
            await _rabbitMqConsumer.StartAsync(stoppingToken);
            
            // Keep running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Audit Worker");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit Worker stopping...");
        await _rabbitMqConsumer.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
