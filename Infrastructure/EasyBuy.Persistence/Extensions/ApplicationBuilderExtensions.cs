using EasyBuy.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Persistence.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UpdateDatabase(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<EasyBuyDbContext>();
        context.Database.Migrate();
    }
}