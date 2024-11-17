using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public OrderItem(Guid orderId, Guid productId, decimal price, int quantity, Order order, Product product,
        Guid id) : base(id)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        OrderId = orderId;
        ProductId = productId;
        Price = price;
        Quantity = quantity;
        Order = order ?? throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        Product = product ?? throw new ArgumentNullException(nameof(product), "Product cannot be null.");
    }

    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public Order Order { get; } // Made read-only
    public Product Product { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    public decimal TotalPrice => Price * Quantity;

    public override bool Equals(object? obj)
    {
        return obj is OrderItem other &&
               OrderId == other.OrderId &&
               ProductId == other.ProductId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OrderId, ProductId);
    }

    public override string ToString()
    {
        return $"{Product.Name} (x{Quantity}) - {Price:C} each, Total: {TotalPrice:C}";
    }
}