using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class OrderItem : BaseEntity<int>
{
    public OrderItem(int orderId, int productId, decimal price, int quantity, Order order, Product product)
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
    
    public int OrderId { get; }
    public int ProductId { get; }
    public Order Order { get; private set; }
    public Product Product { get; private set; } 
    public decimal Price { get; private set; } 
    public int Quantity { get; }

    private decimal GetTotalPrice()
    {
        return Price * Quantity;
    }
    
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
        return $"{Product.Name} (x{Quantity}) - {Price:C} each, Total: {GetTotalPrice():C}";
    }
}