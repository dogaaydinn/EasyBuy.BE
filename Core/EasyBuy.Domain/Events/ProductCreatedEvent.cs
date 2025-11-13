namespace EasyBuy.Domain.Events;

public class ProductCreatedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }

    public ProductCreatedEvent(Guid productId, string productName, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
    }
}
