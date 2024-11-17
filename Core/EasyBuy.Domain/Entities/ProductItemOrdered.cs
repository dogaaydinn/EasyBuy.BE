using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductItemOrdered : Entity<Guid>
{
    public ProductItemOrdered(Guid productId, string productName, Price productPrice, Guid id) : base(id)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID must be a valid GUID.", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(productName));

        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice ??
                       throw new ArgumentNullException(nameof(productPrice), "Product price cannot be null.");
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
    public Price ProductPrice { get; }

    public override string ToString()
    {
        return $"{ProductName} (ID: {ProductId}) - {ProductPrice}";
    }
}