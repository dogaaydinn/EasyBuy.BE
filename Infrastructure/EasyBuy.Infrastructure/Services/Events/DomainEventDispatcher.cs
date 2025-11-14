using EasyBuy.Application.Contracts.Events;
using EasyBuy.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Infrastructure.Services.Events;

/// <summary>
/// Domain event dispatcher implementation using dependency injection to resolve handlers.
/// Provides centralized event dispatching with logging and error handling.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : BaseDomainEvent
    {
        _logger.LogInformation(
            "Dispatching domain event: {EventType} (EventId: {EventId}, OccurredOn: {OccurredOn})",
            typeof(TEvent).Name,
            domainEvent.EventId,
            domainEvent.OccurredOn);

        // Resolve all handlers for this event type
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        var handlersList = handlers.ToList();
        if (!handlersList.Any())
        {
            _logger.LogWarning(
                "No handlers registered for domain event: {EventType}",
                typeof(TEvent).Name);
            return;
        }

        _logger.LogDebug(
            "Found {HandlerCount} handler(s) for event {EventType}",
            handlersList.Count,
            typeof(TEvent).Name);

        // Execute all handlers sequentially
        foreach (var handler in handlersList)
        {
            try
            {
                _logger.LogDebug(
                    "Executing handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    typeof(TEvent).Name);

                await handler.Handle(domainEvent, cancellationToken);

                _logger.LogDebug(
                    "Handler {HandlerType} completed successfully for event {EventType}",
                    handler.GetType().Name,
                    typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error handling domain event {EventType} with handler {HandlerType}. EventId: {EventId}",
                    typeof(TEvent).Name,
                    handler.GetType().Name,
                    domainEvent.EventId);

                // Don't throw - allow other handlers to execute
                // In production, consider dead letter queue or retry mechanism
            }
        }

        _logger.LogInformation(
            "Completed dispatching domain event: {EventType} (EventId: {EventId})",
            typeof(TEvent).Name,
            domainEvent.EventId);
    }

    public async Task DispatchAsync(IEnumerable<BaseDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var eventsList = domainEvents.ToList();

        _logger.LogInformation(
            "Dispatching {EventCount} domain events",
            eventsList.Count);

        foreach (var domainEvent in eventsList)
        {
            // Use reflection to call the generic DispatchAsync method
            var eventType = domainEvent.GetType();
            var method = typeof(DomainEventDispatcher)
                .GetMethod(nameof(DispatchAsync), new[] { eventType, typeof(CancellationToken) });

            if (method != null)
            {
                var task = (Task)method.Invoke(this, new object[] { domainEvent, cancellationToken })!;
                await task;
            }
            else
            {
                _logger.LogWarning(
                    "Could not dispatch event of type {EventType} - method not found",
                    eventType.Name);
            }
        }
    }
}
