using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Abstractions.Storage.Local;
using EasyBuy.Infrastructure.Enums;
using EasyBuy.Infrastructure.Services.Storage;
using EasyBuy.Infrastructure.Services.Storage.Local;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped<ILocalStorage, LocalStorage>();
        }
        public static void AddStorage(this IServiceCollection services, string storageType)
        {
            switch (storageType)
            {
                case StorageType.Local:
                    services.AddScoped<IStorageService, LocalStorage>();
                    break;
                default:
                    services.AddScoped<IStorageService, StorageService>();
                    break;
            }
        }
    }
}