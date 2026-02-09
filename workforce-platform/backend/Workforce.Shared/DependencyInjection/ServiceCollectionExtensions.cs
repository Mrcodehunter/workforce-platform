using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Workforce.Shared.Cache;
using Workforce.Shared.Configuration;
using Workforce.Shared.EventPublisher;

namespace Workforce.Shared.DependencyInjection;

/// <summary>
/// Centralized dependency injection extensions for shared services
/// Provides a single entry point for configuring Redis, RabbitMQ, and other shared infrastructure
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all shared infrastructure services (Redis, RabbitMQ) with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";

        // Add Redis cache with environment-aware configuration
        services.AddRedisCache(configuration, environmentName);

        // Add RabbitMQ publisher with environment-aware configuration
        services.AddRabbitMqPublisher(configuration, environmentName);

        return services;
    }

    /// <summary>
    /// Adds MongoDB database service with environment-aware configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMongoDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";

        var mongoConnection = configuration.GetConnectionString("MongoDB");
        var mongoDatabaseName = configuration["MongoDB:DatabaseName"];

        // Environment-specific defaults
        if (string.IsNullOrEmpty(mongoConnection))
        {
            mongoConnection = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                ? "mongodb://localhost:27017"
                : "mongodb://admin:changeme@mongodb:27017";
        }

        if (string.IsNullOrEmpty(mongoDatabaseName))
        {
            mongoDatabaseName = "workforce_db";
        }

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IMongoDatabase>>();
            try
            {
                logger.LogInformation("Connecting to MongoDB at {ConnectionString} (Environment: {Environment})", 
                    mongoConnection?.Replace(mongoConnection.Contains("@") ? mongoConnection.Split('@')[1] : "", "[REDACTED]"), 
                    environmentName);
                
                var mongoClient = new MongoClient(mongoConnection);
                var database = mongoClient.GetDatabase(mongoDatabaseName);
                
                logger.LogInformation("MongoDB connection established for database: {DatabaseName}", mongoDatabaseName);
                return database;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to MongoDB");
                throw;
            }
        });

        return services;
    }

    /// <summary>
    /// Validates that all required configuration sections are present
    /// Note: This should be called after services are built, not during service registration
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="configuration">The configuration instance</param>
    public static void ValidateSharedConfiguration(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Workforce.Shared.Configuration");
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] 
            ?? configuration["DOTNET_ENVIRONMENT"] 
            ?? "Development";

        // Validate Redis configuration
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
        if (redisOptions != null && redisOptions.Enabled)
        {
            if (string.IsNullOrEmpty(redisOptions.ConnectionString))
            {
                logger.LogWarning("Redis is enabled but ConnectionString is not configured. Using default.");
            }
        }

        // Validate RabbitMQ configuration
        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>();
        if (rabbitMqOptions != null && rabbitMqOptions.Enabled)
        {
            if (string.IsNullOrEmpty(rabbitMqOptions.Host))
            {
                logger.LogWarning("RabbitMQ is enabled but Host is not configured. Using default.");
            }
        }

        logger.LogInformation("Configuration validation completed for environment: {Environment}", environmentName);
    }
}
