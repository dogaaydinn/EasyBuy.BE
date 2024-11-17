using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class BasketItem : Entity<Guid>
{
    public BasketItem(Guid basketId, Guid productId, int quantity, decimal price, Guid id, Basket basket, Product product) : base(id)
    {
        if (basketId == Guid.Empty)
            throw new ArgumentException("Basket ID must be a valid GUID.", nameof(basketId));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID must be a valid GUID.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        BasketId = basketId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        Basket = basket;
        Product = product;
    }

    public Guid BasketId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public decimal Price { get; }

    public Basket Basket { get; set; }
    public Product Product { get; set; }

    public override string ToString()
    {
        return $"Basket ID: {BasketId}, Product ID: {ProductId}, Quantity: {Quantity}, Price: {Price}";
    }
}