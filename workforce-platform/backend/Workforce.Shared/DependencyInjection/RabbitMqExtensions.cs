using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workforce.Shared.EventPublisher;
using Workforce.Shared.Configuration;

namespace Workforce.Shared.DependencyInjection;

/// <summary>
/// Extension methods for configuring RabbitMQ event publisher with dependency injection
/// </summary>
/// <remarks>
/// These extensions provide a clean way to register RabbitMQ services in Program.cs.
/// They handle:
/// 1. Configuration binding from appsettings.json
/// 2. Environment-aware defaults (localhost for dev, rabbitmq for prod)
/// 3. No-op publisher when RabbitMQ is disabled (for local development/testing)
/// </remarks>
public static class RabbitMqExtensions
{
    /// <summary>
    /// Adds RabbitMQ event publisher services to the service collection with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">The configuration instance to read settings from</param>
    /// <param name="environmentName">Optional environment name. If not provided, reads from configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// This method:
    /// 1. Binds RabbitMQ configuration from appsettings.json to RabbitMqOptions
    /// 2. Applies environment-specific defaults (localhost for Development, rabbitmq for Production)
    /// 3. Registers either RabbitMqPublisher (if enabled) or NoOpRabbitMqPublisher (if disabled)
    /// 
    /// The publisher is registered as singleton to reuse the connection across requests.
    /// 
    /// Usage in Program.cs:
    /// <code>
    /// services.AddRabbitMqPublisher(configuration);
    /// </code>
    /// </remarks>
    public static IServiceCollection AddRabbitMqPublisher(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = null)
    {
        // Bind configuration section to RabbitMqOptions for dependency injection
        // This allows other services to inject IOptions&lt;RabbitMqOptions&gt; if needed
        services.Configure<RabbitMqOptions>(options =>
        {
            configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);
        });
        
        // Determine environment name from configuration if not provided
        // Checks both ASPNETCORE_ENVIRONMENT and DOTNET_ENVIRONMENT variables
        environmentName ??= configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";
        
        // Read configured options or create defaults
        // This allows the code to work even if RabbitMQ section is missing from appsettings.json
        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() 
            ?? new RabbitMqOptions();
        
        // Apply environment-specific host defaults
        // Development: localhost (for local RabbitMQ instance)
        // Production: rabbitmq (Docker service name in docker-compose)
        if (string.IsNullOrEmpty(rabbitMqOptions.Host))
        {
            rabbitMqOptions.Host = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "localhost"
                : "rabbitmq";
        }
        
        // Register appropriate publisher implementation based on Enabled flag
        // This allows disabling RabbitMQ for local development without breaking the application
        if (rabbitMqOptions.Enabled)
        {
            // Register real RabbitMQ publisher - establishes connection during construction
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        }
        else
        {
            // Register no-op publisher for environments where RabbitMQ is disabled
            // This allows the application to run without RabbitMQ (useful for testing/local dev)
            // Events will be logged but not published
            services.AddSingleton<IRabbitMqPublisher, NoOpRabbitMqPublisher>();
        }
        
        return services;
    }
}

/// <summary>
/// No-op implementation of IRabbitMqPublisher for environments where RabbitMQ is disabled
/// </summary>
/// <remarks>
/// This implementation allows the application to function without RabbitMQ.
/// All event publishing calls are logged as warnings but no actual messages are sent.
/// 
/// This is useful for:
/// - Local development without RabbitMQ running
/// - Unit testing where you don't want to set up RabbitMQ
/// - Environments where event publishing is intentionally disabled
/// 
/// The implementation still returns an eventId (generated if not provided) to maintain
/// compatibility with code that expects an eventId for correlation.
/// </remarks>
internal class NoOpRabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly ILogger<NoOpRabbitMqPublisher>? _logger;

    /// <summary>
    /// Initializes a new instance of NoOpRabbitMqPublisher
    /// </summary>
    /// <param name="logger">Optional logger for warning messages</param>
    public NoOpRabbitMqPublisher(ILogger<NoOpRabbitMqPublisher>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// No-op event publishing - logs warning and returns eventId
    /// </summary>
    /// <param name="eventType">The event type (logged but not published)</param>
    /// <param name="eventData">The event data (ignored)</param>
    /// <param name="eventId">Optional event ID (generated if not provided)</param>
    /// <param name="cancellationToken">Cancellation token (ignored)</param>
    /// <returns>The eventId (provided or newly generated)</returns>
    /// <remarks>
    /// This method simulates event publishing without actually sending messages.
    /// It logs a warning to indicate that RabbitMQ is disabled, which helps with debugging.
    /// </remarks>
    public Task<string> PublishEventAsync(Workforce.Shared.Events.AuditEventType eventType, object eventData, string? eventId = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogWarning("RabbitMQ is disabled. Event {EventType} was not published.", eventType);
        // Return eventId for compatibility - allows code to still track events even if not published
        return Task.FromResult(eventId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Disposes the no-op publisher (no resources to dispose)
    /// </summary>
    public void Dispose()
    {
        // No-op - no resources to dispose
    }
}
