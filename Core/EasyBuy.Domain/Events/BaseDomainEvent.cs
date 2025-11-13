namespace EasyBuy.Domain.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
