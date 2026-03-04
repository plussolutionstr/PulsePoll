using System.Text.Json;
using PulsePoll.Application.Interfaces;
using StackExchange.Redis;

namespace PulsePoll.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!);
    }

    public Task SetAsync<T>(T value, string key, TimeSpan? expiry = null)
    {
        var serialized = JsonSerializer.Serialize(value);
        return expiry.HasValue
            ? _db.StringSetAsync(key, serialized, expiry.Value)
            : _db.StringSetAsync(key, serialized);
    }

    public Task RemoveAsync(string key)
        => _db.KeyDeleteAsync(key);

    public Task<bool> ExistsAsync(string key)
        => _db.KeyExistsAsync(key);

    public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null)
    {
        var count = await _db.StringIncrementAsync(key);

        // Sadece ilk increment'te TTL set et
        if (count == 1 && expiry.HasValue)
            await _db.KeyExpireAsync(key, expiry.Value);

        return count;
    }
}
