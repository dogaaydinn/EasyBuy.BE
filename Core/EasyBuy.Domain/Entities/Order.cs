using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class Order : BaseEntity
{

    public ICollection<ProductOrder> ProductOrders { get; set; }

    public int AppUserId { get; }
    public AppUser AppUser { get; set; }

    public Guid DeliveryMethodId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal Total { get; set; }
    public Sale Sale { get; set; }

    public List<OrderItem> OrderItems { get; }
    
}