namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for Redis cache connection and behavior
/// </summary>
/// <remarks>
/// This class is bound from the "Redis" configuration section in appsettings.json.
/// It provides strongly-typed access to Redis configuration settings.
/// 
/// Environment-specific defaults:
/// - Development: "localhost:6379"
/// - Production: "redis:6379" (Docker service name)
/// 
/// The configuration supports graceful degradation - if Redis is unavailable,
/// the application continues to function (though without caching).
/// </remarks>
public class RedisOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string
    /// </summary>
    /// <remarks>
    /// Format: "host:port" or "host:port,password=xxx"
    /// Examples:
    /// - "localhost:6379" (local development)
    /// - "redis:6379" (Docker service name)
    /// - "redis.example.com:6379,password=mypassword" (production with auth)
    /// 
    /// If not specified, defaults are applied based on environment:
    /// - Development: "localhost:6379"
    /// - Production: "redis:6379"
    /// </remarks>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    /// <remarks>
    /// Default: 5000ms (5 seconds)
    /// This determines how long to wait when establishing a connection to Redis.
    /// If connection takes longer, it will timeout and the application will continue
    /// without Redis (graceful degradation).
    /// </remarks>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Whether to abort on connection failure
    /// </summary>
    /// <remarks>
    /// Default: false (allows graceful degradation)
    /// 
    /// If true: Application will fail to start if Redis is unavailable.
    /// If false: Application will start even if Redis is unavailable, and will
    /// retry connections automatically. Cache operations will return default values.
    /// 
    /// Set to false for production to ensure application availability even if Redis is down.
    /// </remarks>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Enable or disable Redis caching
    /// </summary>
    /// <remarks>
    /// Default: true
    /// 
    /// If false:
    /// - Redis connection will not be established
    /// - Cache operations will be no-ops (return default values)
    /// - Useful for local development without Redis, or for testing
    /// 
    /// When disabled, the application will function normally but without
    /// the benefits of caching (e.g., audit trail snapshots won't be stored).
    /// </remarks>
    public bool Enabled { get; set; } = true;
}
