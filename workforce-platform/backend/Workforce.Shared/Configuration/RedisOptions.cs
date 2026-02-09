namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for Redis cache
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string (e.g., "redis:6379" or "localhost:6379")
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Connection timeout in milliseconds (default: 5000)
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Whether to abort on connection failure (default: false for graceful degradation)
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Enable Redis for caching (default: true)
    /// If false, cache operations will be no-ops
    /// </summary>
    public bool Enabled { get; set; } = true;
}
