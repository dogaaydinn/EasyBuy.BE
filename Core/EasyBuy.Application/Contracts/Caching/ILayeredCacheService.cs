namespace EasyBuy.Application.Contracts.Caching;

/// <summary>
/// Multi-level caching service interface for enterprise-grade caching strategy.
/// Implements L1 (in-memory) + L2 (Redis) caching layers for optimal performance.
/// </summary>
public interface ILayeredCacheService
{
    /// <summary>
    /// Get value from cache (checks L1 first, then L2)
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set value in both cache layers
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove value from both cache layers
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists in any cache layer
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();

    /// <summary>
    /// Clear all caches (use with caution)
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Warm up cache with frequently accessed data
    /// </summary>
    Task WarmUpAsync(CancellationToken cancellationToken = default);
}

public class CacheStatistics
{
    public long L1Hits { get; set; }
    public long L1Misses { get; set; }
    public long L2Hits { get; set; }
    public long L2Misses { get; set; }
    public int L1EntryCount { get; set; }
    public double L1HitRate => L1Hits + L1Misses > 0
        ? (double)L1Hits / (L1Hits + L1Misses) * 100
        : 0;
    public double L2HitRate => L2Hits + L2Misses > 0
        ? (double)L2Hits / (L2Hits + L2Misses) * 100
        : 0;
}
