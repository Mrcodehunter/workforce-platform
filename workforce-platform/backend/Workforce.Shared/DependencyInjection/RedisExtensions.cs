using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Workforce.Shared.Cache;

namespace Workforce.Shared.DependencyInjection;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "redis:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogInformation("Connecting to Redis at {ConnectionString}", redisConnectionString);
                var config = ConfigurationOptions.Parse(redisConnectionString);
                config.AbortOnConnectFail = false; // Don't fail if Redis is not available
                config.ConnectTimeout = 5000; // 5 second timeout
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger.LogWarning(ex, "Failed to connect to Redis at startup: {Message}. Will retry on first use.", ex.Message);
                // Create a connection that will retry - ConnectionMultiplexer handles reconnection automatically
                var config = ConfigurationOptions.Parse(redisConnectionString);
                config.AbortOnConnectFail = false;
                config.ConnectTimeout = 5000;
                return ConnectionMultiplexer.Connect(config);
            }
        });
        
        services.AddSingleton<IRedisCache, RedisCache>();
        
        return services;
    }
}
