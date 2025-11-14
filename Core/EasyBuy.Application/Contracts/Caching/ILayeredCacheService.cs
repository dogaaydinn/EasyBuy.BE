namespace EasyBuy.Application.Contracts.Caching;

/// <summary>
/// Multi-level cache service interface supporting L1 (memory), L2 (Redis), and L3 (distributed).
/// Provides cache-aside pattern with automatic fallback between layers.
/// </summary>
public interface ILayeredCacheService
{
    /// <summary>
    /// Gets a value from cache, checking all layers in order (L1 → L2 → L3).
    /// Automatically promotes values to higher cache levels (cache warming).
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in all cache layers with specified TTL.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from all cache layers.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys matching the pattern from all cache layers.
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache or fetches it using the provided factory function.
    /// Implements cache-aside pattern with automatic cache population.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Warms the cache by pre-loading frequently accessed data.
    /// </summary>
    Task WarmCacheAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cache layers.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Cache statistics for monitoring and observability.
/// </summary>
public class CacheStatistics
{
    public long L1Hits { get; set; }
    public long L1Misses { get; set; }
    public long L2Hits { get; set; }
    public long L2Misses { get; set; }
    public long L3Hits { get; set; }
    public long L3Misses { get; set; }

    public double L1HitRate => L1Hits + L1Misses > 0 ? (double)L1Hits / (L1Hits + L1Misses) : 0;
    public double L2HitRate => L2Hits + L2Misses > 0 ? (double)L2Hits / (L2Hits + L2Misses) : 0;
    public double L3HitRate => L3Hits + L3Misses > 0 ? (double)L3Hits / (L3Hits + L3Misses) : 0;
    public double OverallHitRate
    {
        get
        {
            var totalHits = L1Hits + L2Hits + L3Hits;
            var totalRequests = totalHits + L1Misses + L2Misses + L3Misses;
            return totalRequests > 0 ? (double)totalHits / totalRequests : 0;
        }
    }
}
