using EasyBuy.Application.Customer;
using EasyBuy.Application.Order;
using EasyBuy.Application.Product;
using EasyBuy.Persistence.Contexts;
using EasyBuy.Persistence.Repositories.Customer;
using EasyBuy.Persistence.Repositories.Order;
using EasyBuy.Persistence.Repositories.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<EasyBuyDbContext>(options =>
            options.UseNpgsql("Password=*******;Host=localhost;Port=5432;Database=postgres;"));


        #region Repository Services
        
        services.AddScoped<IProductReadRepository, EfProductReadRepository>();
        services.AddScoped<IProductWriteRepository, EfProductWriteRepository>();
        services.AddScoped<IOrderReadRepository, EfOrderReadRepository>();
        services.AddScoped<IOrderWriteRepository, EfOrderWriteRepository>();
        services.AddScoped<ICustomerReadRepository, EfCustomerReadRepository>();
        services.AddScoped<ICustomerWriteRepository, EfCustomerWriteRepository>();
        #endregion
    }
}