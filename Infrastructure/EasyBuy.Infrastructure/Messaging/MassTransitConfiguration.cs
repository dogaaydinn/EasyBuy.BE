using EasyBuy.Infrastructure.Messaging.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyBuy.Infrastructure.Messaging;

/// <summary>
/// MassTransit configuration for RabbitMQ message broker.
/// Provides enterprise-grade messaging with automatic retries, dead letter queues, and pub/sub patterns.
/// </summary>
public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(config =>
        {
            // ====================================================================
            // CONSUMER REGISTRATION
            // ====================================================================
            // Register all message consumers
            config.AddConsumer<OrderCreatedConsumer>();
            config.AddConsumer<PaymentProcessedConsumer>();
            config.AddConsumer<InventoryUpdatedConsumer>();

            // ====================================================================
            // RABBITMQ CONFIGURATION
            // ====================================================================
            config.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqPort = configuration.GetValue<ushort>("RabbitMQ:Port", 5672);
                var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";
                var rabbitMqVirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(rabbitMqHost, rabbitMqPort, rabbitMqVirtualHost, h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);

                    // Heartbeat configuration for connection health
                    h.Heartbeat(TimeSpan.FromSeconds(10));

                    // Request timeout
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                });

                // ================================================================
                // MESSAGE RETRY POLICY
                // ================================================================
                // Exponential backoff: 5s, 10s, 30s, 1m, 5m
                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(5),
                    maxInterval: TimeSpan.FromMinutes(5),
                    intervalDelta: TimeSpan.FromSeconds(5)
                ));

                // ================================================================
                // CIRCUIT BREAKER
                // ================================================================
                // Prevent cascading failures
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15; // Trip after 15 failures
                    cb.ActiveThreshold = 10; // Within 10 activations
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                // ================================================================
                // RATE LIMITING
                // ================================================================
                // Prevent overwhelming downstream services
                cfg.UseRateLimit(100, TimeSpan.FromSeconds(1));

                // ================================================================
                // MESSAGE SCHEDULING
                // ================================================================
                // Enable delayed message delivery
                cfg.UseDelayedMessageScheduler();

                // ================================================================
                // ENDPOINT CONFIGURATION
                // ================================================================
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("easybuy", false));

                // ================================================================
                // JSON SERIALIZATION
                // ================================================================
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    return options;
                });
            });
        });

        return services;
    }
}
