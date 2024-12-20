using EasyBuy.Application.Repositories.Customer;
using EasyBuy.Application.Repositories.File;
using EasyBuy.Application.Repositories.Order;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Persistence.Contexts;
using EasyBuy.Persistence.Repositories.Customers;
using EasyBuy.Persistence.Repositories.Files;
using EasyBuy.Persistence.Repositories.Orders;
using EasyBuy.Persistence.Repositories.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services,
        ConfigurationManager builderConfiguration)
    {
        services.AddDbContext<EasyBuyDbContext>(options =>
            options.UseNpgsql("Password=123456;Host=localhost;Port=5432;Database=EasyBuyDB;Username=postgres;"));

        #region Repository Services

        services.AddScoped<IProductReadRepository, EfProductReadRepository>();
        services.AddScoped<IProductWriteRepository, EfProductWriteRepository>();
        services.AddScoped<IOrderReadRepository, EfOrderReadRepository>();
        services.AddScoped<IOrderWriteRepository, EfOrderWriteRepository>();
        services.AddScoped<ICustomerReadRepository, EfCustomerReadRepository>();
        services.AddScoped<ICustomerWriteRepository, EfCustomerWriteRepository>();
        services.AddScoped<IFileReadRepository, EfFileReadRepository>();
        services.AddScoped<IFileWriteRepository, EfFileWriteRepository>();
        services.AddScoped<IInvoiceFileReadRepository, EfInvoiceFileReadRepository>();
        services.AddScoped<IInvoiceFileWriteRepository, EfInvoiceFileWriteRepository>();
        services.AddScoped<IProductImageFileReadRepository, EfProductImageFileReadRepository>();
        services.AddScoped<IProductImageFileWriteRepository, EfProductImageFileWriteRepository>();
        
        #endregion
    }
}