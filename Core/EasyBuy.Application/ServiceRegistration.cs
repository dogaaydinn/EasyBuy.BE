using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Application;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        //services.AddFluentValidationAutoValidation();
        //services.AddFluentValidationClientsideAdapters();
        //services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);
    }
}