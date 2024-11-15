using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class BasketItem : BaseEntity<int>
{
    public BasketItem(int quantity, int productId, Product product, int basketId, Basket basket)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
        Quantity = quantity;
        ProductId = productId;
        Product = product ?? throw new ArgumentNullException(nameof(product));
        BasketId = basketId;
        Basket = basket ?? throw new ArgumentNullException(nameof(basket));
    }

    public required int Quantity { get; init; } = 1; 
    public required int ProductId { get; init; } 
    public required Product Product { get; init; }
    public required int BasketId { get; init; }
    public required Basket Basket { get; init; }
}