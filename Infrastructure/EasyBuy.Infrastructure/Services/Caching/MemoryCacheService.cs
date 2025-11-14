using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EasyBuy.Infrastructure.Services.Caching;

/// <summary>
/// L1 cache implementation using in-memory cache.
/// Provides sub-millisecond latency with LRU eviction.
/// </summary>
public sealed class MemoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys; // Track keys for pattern removal
    private long _hits;
    private long _misses;

    // Configuration
    private const int MaxCacheSize = 10_000; // Maximum 10K entries
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _keys = new ConcurrentDictionary<string, byte>();
    }

    public T? Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            Interlocked.Increment(ref _hits);
            _logger.LogDebug("L1 cache hit for key: {Key}", key);
            return value;
        }

        Interlocked.Increment(ref _misses);
        _logger.LogDebug("L1 cache miss for key: {Key}", key);
        return null;
    }

    public void Set<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry,
            Size = 1, // Each entry counts as 1 unit
            Priority = CacheItemPriority.Normal
        };

        // Eviction callback
        cacheEntryOptions.RegisterPostEvictionCallback((k, v, reason, state) =>
        {
            _keys.TryRemove(k.ToString()!, out _);
            _logger.LogDebug("L1 cache eviction: Key={Key}, Reason={Reason}", k, reason);
        });

        _cache.Set(key, value, cacheEntryOptions);
        _keys.TryAdd(key, 0);

        _logger.LogDebug("L1 cache set: Key={Key}, Expiry={Expiry}", key, expiry ?? DefaultExpiry);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("L1 cache removed: Key={Key}", key);
    }

    public void RemoveByPattern(string pattern)
    {
        var keysToRemove = _keys.Keys.Where(k => IsMatch(k, pattern)).ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        _logger.LogInformation("L1 cache pattern removal: Pattern={Pattern}, Removed={Count}", pattern, keysToRemove.Count);
    }

    public void Clear()
    {
        // Memory cache doesn't have a Clear method, so we remove all tracked keys
        foreach (var key in _keys.Keys.ToList())
        {
            Remove(key);
        }

        _logger.LogInformation("L1 cache cleared");
    }

    public (long Hits, long Misses) GetStatistics()
    {
        return (Interlocked.Read(ref _hits), Interlocked.Read(ref _misses));
    }

    private static bool IsMatch(string key, string pattern)
    {
        // Simple wildcard matching (* and ?)
        pattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(key, pattern);
    }
}
