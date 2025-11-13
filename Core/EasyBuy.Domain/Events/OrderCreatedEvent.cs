namespace EasyBuy.Domain.Events;

public class OrderCreatedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }
    public int ItemCount { get; }

    public OrderCreatedEvent(Guid orderId, Guid userId, decimal totalAmount, int itemCount)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
        ItemCount = itemCount;
    }
}
