using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Workforce.Shared.Cache;
using Workforce.Shared.Configuration;

namespace Workforce.Shared.DependencyInjection;

/// <summary>
/// Extension methods for configuring Redis cache with dependency injection
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Adds Redis cache services to the service collection with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="environmentName">Optional environment name (Development, Production, etc.)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services, 
        IConfiguration configuration,
        string? environmentName = null)
    {
        // Bind configuration to options
        services.Configure<RedisOptions>(options =>
        {
            configuration.GetSection(RedisOptions.SectionName).Bind(options);
        });
        
        // Get environment name if not provided
        environmentName ??= configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";
        
        // Get configured options or use defaults
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() 
            ?? new RedisOptions();
        
        // Environment-specific defaults
        if (string.IsNullOrEmpty(redisOptions.ConnectionString))
        {
            redisOptions.ConnectionString = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "localhost:6379"
                : "redis:6379";
        }
        
        // Register Redis connection (lazy initialization for graceful degradation)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            if (!redisOptions.Enabled)
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogWarning("Redis is disabled in configuration");
                // Return a dummy connection that will fail gracefully
                var config = ConfigurationOptions.Parse("localhost:6379");
                config.AbortOnConnectFail = false;
                config.ConnectTimeout = 100;
                try
                {
                    return ConnectionMultiplexer.Connect(config);
                }
                catch
                {
                    // Will be handled by RedisCache implementation
                    throw new InvalidOperationException("Redis is disabled");
                }
            }

            try
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogInformation("Connecting to Redis at {ConnectionString} (Environment: {Environment})", 
                    redisOptions.ConnectionString, environmentName);
                
                var config = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                config.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                config.ConnectTimeout = redisOptions.ConnectTimeout;
                
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogWarning(ex, 
                    "Failed to connect to Redis at startup: {Message}. Will retry on first use. " +
                    "Application will continue without Redis caching.", ex.Message);
                
                // Create a connection that will retry - ConnectionMultiplexer handles reconnection automatically
                var config = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                config.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                config.ConnectTimeout = redisOptions.ConnectTimeout;
                
                return ConnectionMultiplexer.Connect(config);
            }
        });
        
        // Register Redis cache service
        services.AddSingleton<IRedisCache, RedisCache>();
        
        return services;
    }
}
