using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Configuration;

namespace Workforce.Shared.DependencyInjection;

/// <summary>
/// Extension methods for configuring RabbitMQ with dependency injection
/// </summary>
public static class RabbitMqExtensions
{
    /// <summary>
    /// Adds RabbitMQ event publisher services to the service collection with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="environmentName">Optional environment name (Development, Production, etc.)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRabbitMqPublisher(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = null)
    {
        // Bind configuration to options
        services.Configure<RabbitMqOptions>(options =>
        {
            configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);
        });
        
        // Get environment name if not provided
        environmentName ??= configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";
        
        // Get configured options or use defaults
        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() 
            ?? new RabbitMqOptions();
        
        // Environment-specific defaults
        if (string.IsNullOrEmpty(rabbitMqOptions.Host))
        {
            rabbitMqOptions.Host = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "localhost"
                : "rabbitmq";
        }
        
        // Register RabbitMQ publisher (only if enabled)
        if (rabbitMqOptions.Enabled)
        {
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        }
        else
        {
            // Register a no-op publisher for environments where RabbitMQ is disabled
            services.AddSingleton<IRabbitMqPublisher, NoOpRabbitMqPublisher>();
        }
        
        return services;
    }
}

/// <summary>
/// No-op implementation of IRabbitMqPublisher for environments where RabbitMQ is disabled
/// </summary>
internal class NoOpRabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly ILogger<NoOpRabbitMqPublisher>? _logger;

    public NoOpRabbitMqPublisher(ILogger<NoOpRabbitMqPublisher>? logger = null)
    {
        _logger = logger;
    }

    public Task<string> PublishEventAsync(Workforce.Shared.Events.AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogWarning("RabbitMQ is disabled. Event {EventType} was not published.", eventType);
        return Task.FromResult(eventId ?? Guid.NewGuid().ToString());
    }

    public void Dispose()
    {
        // No-op
    }
}
