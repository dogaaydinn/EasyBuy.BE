using EasyBuy.Application.Contracts.Messaging;

namespace EasyBuy.Application.Messaging.IntegrationEvents;

/// <summary>
/// Integration event published when product inventory is updated.
/// Consumed by: Product Service, Notification Service, Analytics Service
/// </summary>
public sealed class InventoryUpdatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int OldStock { get; init; }
    public int NewStock { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool IsLowStock { get; init; }
}
