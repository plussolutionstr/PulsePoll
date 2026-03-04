namespace PulsePoll.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(T value, string key, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Atomik olarak artırır. İlk çağrıda TTL set eder, sonraki çağrılarda set etmez.
    /// </summary>
    Task<long> IncrementAsync(string key, TimeSpan? expiry = null);
}
