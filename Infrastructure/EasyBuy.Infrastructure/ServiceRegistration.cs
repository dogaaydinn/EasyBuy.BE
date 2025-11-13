using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Azure;
using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Infrastructure.Enums;
using EasyBuy.Infrastructure.Services.Caching;
using EasyBuy.Infrastructure.Services.CurrentUser;
using EasyBuy.Infrastructure.Services.DateTime;
using EasyBuy.Infrastructure.Services.Email;
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

        // Register caching service
        services.AddScoped<ICacheService, RedisCacheService>();

        // Register email service
        services.AddScoped<IEmailService, SendGridEmailService>();

        // Register SMS service
        services.AddScoped<ISmsService, TwilioSmsService>();

        // Register payment service
        services.AddScoped<IPaymentService, StripePaymentService>();

        return services;
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