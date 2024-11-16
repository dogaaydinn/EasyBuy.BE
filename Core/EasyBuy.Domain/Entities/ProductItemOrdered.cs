using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductItemOrdered
{
    public ProductItemOrdered(int productId, string productName, Price productPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("Product ID must be a positive number.", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(productName));

        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice ?? throw new ArgumentNullException(nameof(productPrice), "Product price cannot be null.");
    }
        
    public int ProductId { get; }
    public string ProductName { get; }
    public Price ProductPrice { get; }
        
    public override string ToString()
    {
        return $"{ProductName} (ID: {ProductId}) - {ProductPrice}";
    }
}