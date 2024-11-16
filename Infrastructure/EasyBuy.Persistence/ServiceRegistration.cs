using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<EasyBuyDbContext>(options =>
            options.UseNpgsql("User ID=postgres;Password=da154679;Host=localhost;Port=5432;Database=EasyBuyAPIDb;"));
    }
}