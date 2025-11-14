using EasyBuy.Domain.Common;

namespace EasyBuy.Application.Contracts.Events;

/// <summary>
/// Generic domain event handler interface.
/// Handlers implement this to respond to domain events asynchronously.
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : BaseDomainEvent
{
    /// <summary>
    /// Handles the domain event asynchronously.
    /// </summary>
    /// <param name="domainEvent">The domain event instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
