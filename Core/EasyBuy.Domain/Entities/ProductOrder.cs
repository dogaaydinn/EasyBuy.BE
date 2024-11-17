using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductOrder : Entity<Guid>
{
    public ProductOrder(Guid orderId, Guid productId, Price productPrice, int quantity, Guid? id = null)
        : base(id ?? Guid.NewGuid())  
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID must be valid.", nameof(orderId));
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID must be valid.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        OrderId = orderId;
        ProductId = productId;
        ProductPrice = productPrice ?? throw new ArgumentNullException(nameof(productPrice), "Product price cannot be null.");
        Quantity = quantity;
    }
    
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public Price ProductPrice { get; }
    public int Quantity { get; }

    public override string ToString()
    {
        return $"Order ID: {OrderId}, Product ID: {ProductId}, Quantity: {Quantity}, Price: {ProductPrice}";
    }
}