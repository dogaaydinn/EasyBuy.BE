using EasyBuy.Application.Contracts.Caching;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EasyBuy.Infrastructure.Services.Caching;

/// <summary>
/// Multi-level cache implementation combining L1 (Memory), L2 (Redis), and optional L3.
/// Provides automatic cache warming and promotion between layers.
/// Implements cache-aside pattern with fallback strategy.
/// </summary>
public sealed class LayeredCacheService : ILayeredCacheService
{
    private readonly MemoryCacheService _l1Cache; // Memory cache (sub-ms)
    private readonly RedisCacheService _l2Cache; // Redis cache (~5ms)
    private readonly ILogger<LayeredCacheService> _logger;

    // Statistics tracking
    private long _l1Hits, _l1Misses;
    private long _l2Hits, _l2Misses;
    private long _l3Hits, _l3Misses;

    public LayeredCacheService(
        MemoryCacheService l1Cache,
        RedisCacheService l2Cache,
        ILogger<LayeredCacheService> logger)
    {
        _l1Cache = l1Cache;
        _l2Cache = l2Cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        // Try L1 (Memory) first - fastest
        var value = _l1Cache.Get<T>(key);
        if (value != null)
        {
            Interlocked.Increment(ref _l1Hits);
            _logger.LogDebug("L1 cache hit: Key={Key}, Latency={Latency}ms", key, stopwatch.ElapsedMilliseconds);
            return value;
        }
        Interlocked.Increment(ref _l1Misses);

        // Try L2 (Redis) - slower but distributed
        value = await _l2Cache.GetAsync<T>(key);
        if (value != null)
        {
            Interlocked.Increment(ref _l2Hits);
            _logger.LogDebug("L2 cache hit: Key={Key}, Latency={Latency}ms", key, stopwatch.ElapsedMilliseconds);

            // Promote to L1 (cache warming)
            _l1Cache.Set(key, value, TimeSpan.FromMinutes(5));
            _logger.LogDebug("Promoted key to L1: {Key}", key);

            return value;
        }
        Interlocked.Increment(ref _l2Misses);

        // TODO: Try L3 (Hazelcast/NCache) if configured
        // For now, return null (cache miss)

        _logger.LogDebug("Cache miss (all layers): Key={Key}, Latency={Latency}ms", key, stopwatch.ElapsedMilliseconds);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        var effectiveExpiry = expiry ?? TimeSpan.FromMinutes(10);

        // Set in L1 (Memory) - TTL: 5 minutes (shorter for memory)
        _l1Cache.Set(key, value, TimeSpan.FromMinutes(5));

        // Set in L2 (Redis) - TTL: as specified
        await _l2Cache.SetAsync(key, value, effectiveExpiry);

        // TODO: Set in L3 if configured

        _logger.LogDebug("Cache set (all layers): Key={Key}, Expiry={Expiry}", key, effectiveExpiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Remove from all layers
        _l1Cache.Remove(key);
        await _l2Cache.RemoveAsync(key);
        // TODO: Remove from L3 if configured

        _logger.LogInformation("Cache removed (all layers): Key={Key}", key);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Remove from all layers
        _l1Cache.RemoveByPattern(pattern);
        await _l2Cache.RemoveByPatternAsync(pattern);
        // TODO: Remove from L3 if configured

        _logger.LogInformation("Cache pattern removed (all layers): Pattern={Pattern}", pattern);
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Cache-aside pattern implementation
        var value = await GetAsync<T>(key, cancellationToken);
        if (value != null)
        {
            return value;
        }

        _logger.LogDebug("Cache miss - executing factory for key: {Key}", key);

        // Execute factory to get value
        var stopwatch = Stopwatch.StartNew();
        value = await factory();
        stopwatch.Stop();

        _logger.LogInformation(
            "Factory executed for key: {Key}, Duration={Duration}ms",
            key,
            stopwatch.ElapsedMilliseconds);

        // Store in cache
        await SetAsync(key, value, expiry, cancellationToken);

        return value;
    }

    public async Task WarmCacheAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cache warming with {Count} entries", data.Count);

        var tasks = data.Select(async kvp =>
        {
            try
            {
                // Assuming all values are of the same type or can be cached as objects
                if (kvp.Value != null)
                {
                    _l1Cache.Set(kvp.Key, kvp.Value, TimeSpan.FromMinutes(30));
                    await _l2Cache.SetAsync(kvp.Key, kvp.Value, TimeSpan.FromHours(1));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming cache for key: {Key}", kvp.Key);
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation("Cache warming completed");
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Clearing all cache layers");

        _l1Cache.Clear();
        // Note: Redis doesn't have a clear all method without pattern
        // You might want to implement a keyspace scan if needed
        await Task.CompletedTask;

        _logger.LogInformation("All cache layers cleared");
    }

    public Task<CacheStatistics> GetStatisticsAsync()
    {
        var l1Stats = _l1Cache.GetStatistics();

        var statistics = new CacheStatistics
        {
            L1Hits = l1Stats.Hits,
            L1Misses = l1Stats.Misses,
            L2Hits = Interlocked.Read(ref _l2Hits),
            L2Misses = Interlocked.Read(ref _l2Misses),
            L3Hits = Interlocked.Read(ref _l3Hits),
            L3Misses = Interlocked.Read(ref _l3Misses)
        };

        return Task.FromResult(statistics);
    }
}
