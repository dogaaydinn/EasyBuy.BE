
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class OrderItem : BaseEntity<int>
{
    public int OrderId { get; set; } // Foreign Key
    public Order Order { get; set; } 
    public int ProductId { get; set; } // Foreign Key
    public Product Product { get; set; } 
    public decimal Price { get; set; } // Ensure Price >= 0
    public int Quantity { get; set; } // Ensure Quantity > 0
}
