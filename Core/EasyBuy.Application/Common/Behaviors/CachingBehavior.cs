using EasyBuy.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EasyBuy.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for caching query results
/// Applies only to requests that implement ICacheableQuery
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = GetCacheKey(request, cacheableQuery.CacheKey);

        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);

        var response = await next();

        await _cacheService.SetAsync(
            cacheKey,
            response,
            cacheableQuery.CacheExpiration,
            cancellationToken);

        return response;
    }

    private static string GetCacheKey(TRequest request, string baseCacheKey)
    {
        var requestJson = JsonSerializer.Serialize(request);
        var requestHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(requestJson)));

        return $"{baseCacheKey}:{requestHash}";
    }
}

/// <summary>
/// Marker interface for queries that should be cached
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheExpiration { get; }
}
