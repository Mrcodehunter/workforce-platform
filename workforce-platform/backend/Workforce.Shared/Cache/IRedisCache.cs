namespace Workforce.Shared.Cache;

/// <summary>
/// Interface for Redis cache operations
/// Provides a type-safe abstraction over StackExchange.Redis for caching audit trail snapshots
/// and other application data.
/// </summary>
/// <remarks>
/// This interface is used to store "before" and "after" snapshots of entities for audit logging.
/// The implementation handles JSON serialization/deserialization automatically.
/// </remarks>
public interface IRedisCache
{
    /// <summary>
    /// Stores a value in Redis cache with optional expiration
    /// </summary>
    /// <typeparam name="T">The type of value to store (will be JSON serialized)</typeparam>
    /// <param name="key">The cache key (e.g., "audit:{eventId}:before")</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time (default: no expiration)</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="Exception">Thrown if Redis connection fails or serialization fails</exception>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Retrieves a value from Redis cache
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve (will be JSON deserialized)</typeparam>
    /// <param name="key">The cache key to retrieve</param>
    /// <returns>The cached value, or default(T) if key doesn't exist</returns>
    /// <remarks>
    /// Returns default(T) (null for reference types) if key doesn't exist or deserialization fails.
    /// This allows graceful degradation when Redis is unavailable.
    /// </remarks>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Deletes a key from Redis cache
    /// </summary>
    /// <param name="key">The cache key to delete</param>
    /// <returns>Task representing the async operation</returns>
    /// <remarks>
    /// Used to clean up audit snapshot keys after they've been processed by the audit logger worker.
    /// </remarks>
    Task DeleteAsync(string key);

    /// <summary>
    /// Checks if a key exists in Redis cache
    /// </summary>
    /// <param name="key">The cache key to check</param>
    /// <returns>True if key exists, false otherwise</returns>
    /// <remarks>
    /// Returns false if Redis is unavailable, allowing graceful degradation.
    /// </remarks>
    Task<bool> ExistsAsync(string key);
}
