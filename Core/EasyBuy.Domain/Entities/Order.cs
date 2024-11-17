using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class Order : Entity<Guid>
{
    public Order(int appUserId, Guid deliveryMethodId, OrderStatus orderStatus, decimal total, AppUser appUser,
        DeliveryMethod deliveryMethod, Guid id, ICollection<ProductOrder> productOrders, Sale sale) : base(id)
    {
        if (appUserId <= 0) throw new ArgumentException("AppUserId must be greater than zero.", nameof(appUserId));
        if (deliveryMethodId == Guid.Empty)
            throw new ArgumentException("DeliveryMethodId must be a valid GUID.", nameof(deliveryMethodId));
        if (total < 0) throw new ArgumentException("Total cannot be negative.", nameof(total));

        AppUserId = appUserId;
        DeliveryMethodId = deliveryMethodId;
        OrderStatus = orderStatus;
        OrderDate = DateTime.UtcNow;
        Total = total;
        AppUser = appUser ?? throw new ArgumentNullException(nameof(appUser), "AppUser cannot be null.");
        DeliveryMethod = deliveryMethod ??
                         throw new ArgumentNullException(nameof(deliveryMethod), "DeliveryMethod cannot be null.");
        ProductOrders = productOrders;
        Sale = sale;
        OrderItems = new List<OrderItem>();
    }

    public ICollection<ProductOrder> ProductOrders { get; set; }

    public int AppUserId { get; }
    public AppUser AppUser { get; private set; }

    public Guid DeliveryMethodId { get; private set; }
    public DeliveryMethod DeliveryMethod { get; private set; }

    public OrderStatus OrderStatus { get; private set; }

    public DateTime OrderDate { get; private set; }

    public decimal Total { get; private set; }
    public Sale Sale { get; private set; }

    public List<OrderItem> OrderItems { get; }

    public void SetSale(Sale sale)
    {
        Sale = sale ?? throw new ArgumentNullException(nameof(sale), "Sale cannot be null.");
    }

    public IReadOnlyCollection<OrderItem> GetOrderItems()
    {
        return OrderItems.AsReadOnly();
    }

    public void AddItem(OrderItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item), "Order item cannot be null.");
        OrderItems.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(OrderItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item), "Order item cannot be null.");
        if (!OrderItems.Contains(item)) throw new ArgumentException("Item not found in the order.", nameof(item));
        OrderItems.Remove(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = 0;
        foreach (var item in OrderItems) Total += item.TotalPrice;
    }

    public override string ToString()
    {
        return $"Order {Id} for User {AppUserId} with {OrderItems.Count} items, Total: {Total:C}.";
    }
}