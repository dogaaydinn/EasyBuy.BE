
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class BasketItem : BaseEntity<int>
{
    public int Quantity { get; set; } // Ensure Quantity > 0 with validation.
    public int ProductId { get; set; } // Foreign Key
    public Product Product { get; set; } 
    public int BasketId { get; set; } // Foreign Key
    public Basket Basket { get; set; } 
}

