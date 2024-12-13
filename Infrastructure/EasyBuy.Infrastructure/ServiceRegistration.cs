using EasyBuy.Application.Abstractions.Storage;
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
    }

    public static void AddStorage<T>(this IServiceCollection serviceCollection) where T : class, IStorageService
    {
        serviceCollection.AddScoped<IStorageService, T>();
    }

    public static void AddStorage(this IServiceCollection services, StorageType storageType)
    {
        switch (storageType)
        {
            case StorageType.Local:
                services.AddScoped<IStorageService, LocalStorage>();
                break;
            case StorageType.Azure:
                services.AddScoped<IStorageService, AzureStorage>(); 
                break;
            default:
                services.AddScoped<IStorageService, StorageService>(); 
                break;
        }
    }
}