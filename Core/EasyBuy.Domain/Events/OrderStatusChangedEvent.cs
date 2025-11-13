using EasyBuy.Domain.Enums;

namespace EasyBuy.Domain.Events;

public class OrderStatusChangedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
    public string? Reason { get; }

    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, string? reason = null)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}
