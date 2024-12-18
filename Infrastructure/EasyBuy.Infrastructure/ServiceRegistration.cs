using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Azure;
using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Infrastructure.Enums;
using EasyBuy.Infrastructure.Services.Storage;
using EasyBuy.Infrastructure.Services.Storage.Azure;
using EasyBuy.Infrastructure.Services.Storage.Local;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<ILocalStorage, LocalStorage>();
        services.AddScoped<IAzureStorage, AzureStorage>();
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