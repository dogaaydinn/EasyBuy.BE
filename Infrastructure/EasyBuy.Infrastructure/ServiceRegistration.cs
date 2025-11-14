using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Azure;
using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Infrastructure.Enums;
using EasyBuy.Infrastructure.Services.Auth;
using EasyBuy.Infrastructure.Services.Basket;
using EasyBuy.Infrastructure.Services.Caching;
using EasyBuy.Infrastructure.Services.CurrentUser;
using EasyBuy.Infrastructure.Services.DateTime;
using EasyBuy.Infrastructure.Services.Email;
using EasyBuy.Infrastructure.Services.Events;
using EasyBuy.Infrastructure.Services.Payment;
using EasyBuy.Infrastructure.Services.Sms;
using EasyBuy.Infrastructure.Services.Storage;
using EasyBuy.Infrastructure.Services.Storage.Azure;
using EasyBuy.Infrastructure.Services.Storage.Local;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Storage services
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<ILocalStorage, LocalStorage>();
        services.AddScoped<IAzureStorage, AzureStorage>();

        // Register HttpContextAccessor (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // Register common services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTime, DateTimeService>();

        // ====================================================================
        // MULTI-LEVEL CACHING (L1: Memory, L2: Redis, L3: Optional)
        // ====================================================================
        // L1: In-memory cache (sub-millisecond latency, 10K entries max)
        services.AddSingleton<MemoryCacheService>();

        // L2: Redis cache (existing implementation)
        services.AddScoped<RedisCacheService>();

        // Register both single-level and layered cache services
        services.AddScoped<ICacheService, RedisCacheService>(); // Legacy/simple cache
        services.AddScoped<ILayeredCacheService, LayeredCacheService>(); // Multi-level cache

        // ====================================================================
        // BASKET SERVICE (Redis-based with 30-day expiration)
        // ====================================================================
        services.AddScoped<IBasketService, RedisBasketService>();

        // Register email service
        services.AddScoped<IEmailService, SendGridEmailService>();

        // Register SMS service
        services.AddScoped<ISmsService, TwilioSmsService>();

        // Register payment service
        services.AddScoped<IPaymentService, StripePaymentService>();

        // Register authentication service
        services.AddScoped<ITokenService, JwtTokenService>();

        // ====================================================================
        // EVENT-DRIVEN ARCHITECTURE
        // ====================================================================
        // Register domain event dispatcher for async event processing
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // ====================================================================
        // HANGFIRE BACKGROUND JOBS
        // ====================================================================
        // Register all background jobs for Hangfire
        services.AddTransient<BackgroundJobs.AbandonedCartReminderJob>();
        services.AddTransient<BackgroundJobs.DailySalesReportJob>();
        services.AddTransient<BackgroundJobs.CleanupExpiredTokensJob>();
        services.AddTransient<BackgroundJobs.InventorySynchronizationJob>();
        services.AddTransient<BackgroundJobs.EmailQueueProcessorJob>();
        services.AddSingleton<BackgroundJobs.JobScheduler>();

        return services;
    }

    /// <summary>
    /// Adds MassTransit with RabbitMQ configuration for distributed messaging.
    /// Call this from Program.cs with configuration:
    /// builder.Services.AddMassTransitMessaging(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Import MassTransitConfiguration
        return Messaging.MassTransitConfiguration.AddMassTransitWithRabbitMq(services, configuration);
    }

    public static void AddStorage<T>(this IServiceCollection serviceCollection) where T : class, IStorage
    {
        serviceCollection.AddScoped<IStorage, T>();
    }

    // This is an alternative way to register the storage services
    public static void AddStorage(this IServiceCollection services, StorageType storageType)
    {
        switch (storageType)
        {
            case StorageType.Local:
                services.AddScoped<IStorage, LocalStorage>();
                break;
            case StorageType.Azure:
                services.AddScoped<IStorage, AzureStorage>();
                break;
            default:
                services.AddScoped<IStorage, StorageService>();
                break;
        }
    }
}