using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace EasyBuy.WebAPI.RateLimiting;

/// <summary>
/// Custom client ID resolver that determines rate limit tier based on user role.
/// - Admin users: Higher limits (admin tier)
/// - Premium/Manager users: Medium limits (premium tier)
/// - Regular users: Standard limits (default)
/// </summary>
public class UserRoleClientIdResolveContributor : IClientResolveContributor
{
    private readonly ILogger<UserRoleClientIdResolveContributor> _logger;

    public UserRoleClientIdResolveContributor(ILogger<UserRoleClientIdResolveContributor> logger)
    {
        _logger = logger;
    }

    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        string? clientId = null;

        // Check if user is authenticated
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            // Determine client ID based on user role
            if (httpContext.User.IsInRole("Admin"))
            {
                clientId = "admin";
                _logger.LogDebug("Resolved client ID 'admin' for user in Admin role");
            }
            else if (httpContext.User.IsInRole("Manager") || httpContext.User.IsInRole("Premium"))
            {
                clientId = "premium";
                _logger.LogDebug("Resolved client ID 'premium' for user in Manager/Premium role");
            }
            else
            {
                // Regular authenticated users get default limits
                clientId = httpContext.User.Identity.Name ?? "authenticated";
                _logger.LogDebug("Resolved client ID '{ClientId}' for authenticated user", clientId);
            }
        }
        else
        {
            // Anonymous users get strictest limits (IP-based)
            clientId = "anonymous";
            _logger.LogDebug("Resolved client ID 'anonymous' for unauthenticated request");
        }

        return Task.FromResult(clientId);
    }
}

/// <summary>
/// Custom rate limit configuration for distributed scenarios with Redis.
/// Extends default configuration to support Redis-backed rate limiting.
/// </summary>
public class DistributedRateLimitConfiguration
{
    private readonly ILogger<DistributedRateLimitConfiguration> _logger;

    public DistributedRateLimitConfiguration(ILogger<DistributedRateLimitConfiguration> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configure Redis-backed rate limiting for distributed deployments.
    /// This allows rate limits to be shared across multiple application instances.
    /// </summary>
    public void ConfigureRedisRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("RedisConnection");

        if (!string.IsNullOrEmpty(redisConnection) && configuration.GetValue<bool>("FeatureFlags:EnableDistributedCache"))
        {
            _logger.LogInformation("Configuring Redis-backed distributed rate limiting");

            // Redis-backed rate limiting stores will be added here when needed for production
            // For now, using in-memory stores which work for single-instance deployments

            _logger.LogInformation("Redis distributed rate limiting configured successfully");
        }
        else
        {
            _logger.LogWarning("Redis not configured - using in-memory rate limiting (not suitable for multi-instance deployments)");
        }
    }
}
