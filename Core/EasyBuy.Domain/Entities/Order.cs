using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public Order(int appUserId, int deliveryMethodId, OrderStatus orderStatus, decimal total)
    {
        if (appUserId <= 0) throw new ArgumentException("AppUserId must be greater than zero.", nameof(appUserId));
        if (deliveryMethodId <= 0)
            throw new ArgumentException("DeliveryMethodId must be greater than zero.", nameof(deliveryMethodId));
        if (total < 0) throw new ArgumentException("Total cannot be negative.", nameof(total));

        AppUserId = appUserId;
        DeliveryMethodId = deliveryMethodId;
        OrderStatus = orderStatus;
        OrderDate = DateTime.UtcNow;
        Total = total;
        OrderItems = new List<OrderItem>();
    }

    public int AppUserId { get; }
    public AppUser AppUser { get; private set; } 

    public int DeliveryMethodId { get; private set; }
    public DeliveryMethod DeliveryMethod { get; private set; }

    public OrderStatus OrderStatus { get; private set; }

    public DateTime OrderDate { get; private set; }

    public decimal Total { get; }

    public IReadOnlyCollection<OrderItem> OrderItems { get; }

    // Business Logic: Add an item to the order
    // Business Logic: Remove an item from the order
    // Recalculate the total amount of the order
    
    public override string ToString()
    {
        return $"Order {Id} for User {AppUserId} with {OrderItems.Count} items, Total: {Total:C}.";
    }
}