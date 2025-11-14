namespace EasyBuy.Application.Contracts.Messaging;

/// <summary>
/// Marker interface for integration events that cross service boundaries.
/// Integration events are published to message brokers (RabbitMQ) for inter-service communication.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for the integration event.
    /// Used for idempotency and deduplication.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
