using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Order : BaseEntity
{
    public decimal Total { get; set; }
    public required Delivery Delivery { get; set; }
    public required OrderStatus OrderStatus { get; set; } // Value Object
    public DateTime OrderDate { get; set; } // Value Object
    public required AppUser AppUser { get; set; } // Value Object

    public ICollection<Product> Products { get; set; } = new List<Product>(); // Value Object
}