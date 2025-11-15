namespace EasyBuy.WebAPI.Monitoring;

/// <summary>
/// Application Insights and monitoring configuration
/// </summary>
public static class ApplicationInsightsConfiguration
{
    /// <summary>
    /// Configure Application Insights telemetry
    /// </summary>
    public static IServiceCollection AddApplicationInsightsMonitoring(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = connectionString;
                options.EnableAdaptiveSampling = true;
                options.EnablePerformanceCounterCollectionModule = true;
                options.EnableQuickPulseMetricStream = true;
            });

            // Add custom telemetry
            services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
        }

        return services;
    }
}

/// <summary>
/// Custom telemetry initializer to add additional properties
/// </summary>
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Add correlation ID
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            telemetry.Context.GlobalProperties["CorrelationId"] = correlationId.ToString();
        }

        // Add user information
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            telemetry.Context.User.AuthenticatedUserId = context.User.Identity.Name;
        }

        // Add environment
        telemetry.Context.GlobalProperties["Environment"] =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
    }
}
