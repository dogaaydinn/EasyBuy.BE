using System.Reflection;
using EasyBuy.Application.Common.Behaviors;
using EasyBuy.Application.Contracts.Events;
using EasyBuy.Application.Features.Events.Handlers;
using EasyBuy.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register MediatR pipeline behaviors (order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // ====================================================================
        // DOMAIN EVENT HANDLERS (Event-Driven Architecture)
        // ====================================================================
        // Register all domain event handlers for asynchronous event processing
        services.AddTransient<IDomainEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
        services.AddTransient<IDomainEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
        services.AddTransient<IDomainEventHandler<OrderStatusChangedEvent>, OrderStatusChangedEventHandler>();
        services.AddTransient<IDomainEventHandler<ProductInventoryChangedEvent>, ProductInventoryChangedEventHandler>();
        services.AddTransient<IDomainEventHandler<PaymentProcessedEvent>, PaymentProcessedEventHandler>();
        services.AddTransient<IDomainEventHandler<ProductCreatedEvent>, ProductCreatedEventHandler>();

        return services;
    }
}