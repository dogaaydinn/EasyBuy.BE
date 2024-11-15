using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public int AppUserId { get; set; } // Foreign Key
    public AppUser AppUser { get; set; } // Navigation Property
    public int DeliveryMethodId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public OrderStatus OrderStatus { get; set; } 
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
