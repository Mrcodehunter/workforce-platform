using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Workforce.Shared.Cache;

public class RedisCache : IRedisCache, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCache> _logger;

    public RedisCache(IConnectionMultiplexer redis, ILogger<RedisCache> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Redis key {Key}", key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default(T);
            }

            var jsonString = value.ToString();
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Redis key {Key}", key);
            return default(T);
        }
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Redis key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Redis key existence {Key}", key);
            return false;
        }
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }
}
