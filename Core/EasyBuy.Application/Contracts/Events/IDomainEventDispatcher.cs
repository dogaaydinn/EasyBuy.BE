using EasyBuy.Domain.Common;

namespace EasyBuy.Application.Contracts.Events;

/// <summary>
/// Domain event dispatcher for publishing events to registered handlers.
/// Supports both synchronous and asynchronous event processing.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event to all registered handlers.
    /// Handlers are executed sequentially.
    /// </summary>
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : BaseDomainEvent;

    /// <summary>
    /// Dispatches multiple domain events in order.
    /// </summary>
    Task DispatchAsync(IEnumerable<BaseDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
