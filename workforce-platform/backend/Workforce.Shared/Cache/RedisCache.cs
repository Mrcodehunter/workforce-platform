using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Workforce.Shared.Cache;

/// <summary>
/// Redis cache implementation using StackExchange.Redis
/// Handles JSON serialization/deserialization for storing audit trail snapshots
/// </summary>
/// <remarks>
/// This implementation provides graceful degradation - if Redis is unavailable,
/// GetAsync and ExistsAsync return default values instead of throwing exceptions.
/// This ensures the application continues to function even if Redis is down.
/// </remarks>
public class RedisCache : IRedisCache, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCache> _logger;

    /// <summary>
    /// Initializes a new instance of RedisCache
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer (injected via DI)</param>
    /// <param name="logger">Logger for error tracking</param>
    /// <remarks>
    /// The connection multiplexer is shared across the application and handles
    /// connection pooling and reconnection automatically.
    /// </remarks>
    public RedisCache(IConnectionMultiplexer redis, ILogger<RedisCache> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    /// <summary>
    /// Stores a value in Redis cache with optional expiration
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <exception cref="Exception">Thrown if Redis connection fails or serialization fails</exception>
    /// <remarks>
    /// Values are JSON serialized before storage. For audit snapshots, expiration is typically
    /// set to 1 hour to allow the audit logger worker time to process the event.
    /// </remarks>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            // Serialize the value to JSON for storage in Redis
            // This allows storing complex objects as strings
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            // Log error but rethrow - SetAsync failures should be visible to caller
            // This is different from GetAsync which returns default for graceful degradation
            _logger.LogError(ex, "Error setting Redis key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a value from Redis cache
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve</typeparam>
    /// <param name="key">The cache key to retrieve</param>
    /// <returns>The cached value, or default(T) if key doesn't exist or error occurs</returns>
    /// <remarks>
    /// Returns default(T) on errors to allow graceful degradation when Redis is unavailable.
    /// This ensures the audit logger worker can continue processing even if Redis is down,
    /// though it won't have access to before/after snapshots in that case.
    /// </remarks>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                // Key doesn't exist - return default (null for reference types)
                return default(T);
            }

            // Convert RedisValue to string explicitly to avoid ambiguity
            // Then deserialize from JSON back to the requested type
            var jsonString = value.ToString();
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (Exception ex)
        {
            // Log error but return default instead of throwing
            // This allows graceful degradation - application continues even if Redis fails
            _logger.LogError(ex, "Error getting Redis key {Key}", key);
            return default(T);
        }
    }

    /// <summary>
    /// Deletes a key from Redis cache
    /// </summary>
    /// <param name="key">The cache key to delete</param>
    /// <remarks>
    /// Used to clean up audit snapshot keys after they've been processed by the audit logger.
    /// Errors are logged but not thrown to prevent cleanup failures from breaking the workflow.
    /// </remarks>
    public async Task DeleteAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            // Log but don't throw - cleanup failures shouldn't break the application
            // The key will expire naturally if deletion fails
            _logger.LogError(ex, "Error deleting Redis key {Key}", key);
        }
    }

    /// <summary>
    /// Checks if a key exists in Redis cache
    /// </summary>
    /// <param name="key">The cache key to check</param>
    /// <returns>True if key exists, false otherwise or on error</returns>
    /// <remarks>
    /// Returns false on errors to allow graceful degradation. This is used by the audit logger
    /// to verify snapshot keys exist before attempting to read them.
    /// </remarks>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            // Log error but return false - allows graceful degradation
            _logger.LogError(ex, "Error checking Redis key existence {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Disposes the Redis connection
    /// </summary>
    /// <remarks>
    /// The connection multiplexer is typically a singleton, so this disposal
    /// is usually handled by the DI container at application shutdown.
    /// </remarks>
    public void Dispose()
    {
        _redis?.Dispose();
    }
}
