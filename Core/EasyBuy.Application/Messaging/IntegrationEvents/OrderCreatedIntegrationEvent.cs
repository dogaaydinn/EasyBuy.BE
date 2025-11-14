using EasyBuy.Application.Contracts.Messaging;

namespace EasyBuy.Application.Messaging.IntegrationEvents;

/// <summary>
/// Integration event published when a new order is created.
/// Consumed by: Inventory Service, Notification Service, Analytics Service
/// </summary>
public sealed class OrderCreatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    public string PaymentMethod { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
}

public sealed class OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
