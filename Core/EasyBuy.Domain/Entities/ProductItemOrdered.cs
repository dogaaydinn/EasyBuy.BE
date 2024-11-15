using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductItemOrdered
{
    public int ProductId { get; }
    public string ProductName { get; }
    public Price ProductPrice { get; }

    public ProductItemOrdered(int productId, string productName, Price productPrice)
    {
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
    }
}