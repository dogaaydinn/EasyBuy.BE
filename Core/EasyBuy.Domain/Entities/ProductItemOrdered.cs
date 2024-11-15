using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class ProductItemOrdered(int productId, string productName, Price productPrice)
{
    public int ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public Price ProductPrice { get; } = productPrice;
}