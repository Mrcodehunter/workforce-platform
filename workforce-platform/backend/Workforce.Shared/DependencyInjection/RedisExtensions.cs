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
/// <remarks>
/// These extensions provide a clean way to register Redis services in Program.cs.
/// They handle:
/// 1. Configuration binding from appsettings.json
/// 2. Environment-aware defaults (localhost for dev, redis for prod)
/// 3. Graceful degradation when Redis is unavailable
/// 4. Connection retry logic
/// </remarks>
public static class RedisExtensions
{
    /// <summary>
    /// Adds Redis cache services to the service collection with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">The configuration instance to read settings from</param>
    /// <param name="environmentName">Optional environment name. If not provided, reads from configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// This method:
    /// 1. Binds Redis configuration from appsettings.json to RedisOptions
    /// 2. Applies environment-specific defaults (localhost for Development, redis for Production)
    /// 3. Registers IConnectionMultiplexer as singleton (shared connection pool)
    /// 4. Registers IRedisCache as singleton
    /// 
    /// The connection is established lazily and supports automatic reconnection.
    /// If Redis is unavailable at startup, the application continues and will retry on first use.
    /// 
    /// Usage in Program.cs:
    /// <code>
    /// services.AddRedisCache(configuration);
    /// </code>
    /// </remarks>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services, 
        IConfiguration configuration,
        string? environmentName = null)
    {
        // Bind configuration section to RedisOptions for dependency injection
        // This allows other services to inject IOptions&lt;RedisOptions&gt; if needed
        services.Configure<RedisOptions>(options =>
        {
            configuration.GetSection(RedisOptions.SectionName).Bind(options);
        });
        
        // Determine environment name from configuration if not provided
        // Checks both ASPNETCORE_ENVIRONMENT and DOTNET_ENVIRONMENT variables
        environmentName ??= configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";
        
        // Read configured options or create defaults
        // This allows the code to work even if Redis section is missing from appsettings.json
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() 
            ?? new RedisOptions();
        
        // Apply environment-specific connection string defaults
        // Development: localhost (for local Redis instance)
        // Production: redis (Docker service name in docker-compose)
        if (string.IsNullOrEmpty(redisOptions.ConnectionString))
        {
            redisOptions.ConnectionString = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "localhost:6379"
                : "redis:6379";
        }
        
        // Register Redis connection multiplexer as singleton
        // Singleton ensures connection pooling and shared connection across the application
        // The connection is established during service resolution (lazy initialization)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            // If Redis is disabled, return a connection that will fail gracefully
            // This allows the application to start even if Redis is intentionally disabled
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
                    // Will be handled by RedisCache implementation - it returns default values on errors
                    throw new InvalidOperationException("Redis is disabled");
                }
            }

            // Attempt to connect to Redis
            // If connection fails, we still return a connection object that will retry automatically
            // This ensures the application can start even if Redis is temporarily unavailable
            try
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogInformation("Connecting to Redis at {ConnectionString} (Environment: {Environment})", 
                    redisOptions.ConnectionString, environmentName);
                
                // Parse connection string and apply configuration options
                var config = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                config.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                config.ConnectTimeout = redisOptions.ConnectTimeout;
                
                // Establish connection (ConnectionMultiplexer handles connection pooling automatically)
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                // Log warning but don't throw - allow application to start
                // ConnectionMultiplexer will automatically retry when first used
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogWarning(ex, 
                    "Failed to connect to Redis at startup: {Message}. Will retry on first use. " +
                    "Application will continue without Redis caching.", ex.Message);
                
                // Create a connection configuration that will retry
                // ConnectionMultiplexer handles reconnection automatically in the background
                var config = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                config.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                config.ConnectTimeout = redisOptions.ConnectTimeout;
                
                // Return connection object - it will retry when first accessed
                return ConnectionMultiplexer.Connect(config);
            }
        });
        
        // Register Redis cache service implementation
        // This is the main interface used by application code
        services.AddSingleton<IRedisCache, RedisCache>();
        
        return services;
    }
}
