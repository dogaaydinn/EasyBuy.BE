using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Events;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

/// <summary>
/// Represents a customer order in the e-commerce system.
/// Implements proper aggregate root with OrderItems for order line tracking.
/// </summary>
public class Order : BaseEntity
{
    public required string OrderNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public required Guid DeliveryId { get; set; }
    public required Delivery Delivery { get; set; }

    public required OrderStatus OrderStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CancelledDate { get; set; }

    public required string AppUserId { get; set; }
    public required AppUser AppUser { get; set; }

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Order items - proper many-to-many with additional properties
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Calculate order total from line items
    /// </summary>
    public void CalculateTotal()
    {
        Subtotal = OrderItems.Sum(item => item.TotalPrice);
        Total = Subtotal + TaxAmount + ShippingCost - DiscountAmount;
    }

    /// <summary>
    /// Mark order as shipped with domain event
    /// </summary>
    public void MarkAsShipped(string trackingNumber)
    {
        if (OrderStatus != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be marked as shipped");

        OrderStatus = OrderStatus.Shipped;
        ShippedDate = DateTime.UtcNow;
        TrackingNumber = trackingNumber;

        // Raise domain event for notifications
        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            AppUserId,
            OrderStatus.Shipped,
            AppUser.Email,
            AppUser.PhoneNumber));
    }

    /// <summary>
    /// Cancel order with validation
    /// </summary>
    public void Cancel(string reason)
    {
        if (OrderStatus == OrderStatus.Delivered || OrderStatus == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order with status {OrderStatus}");

        OrderStatus = OrderStatus.Cancelled;
        CancelledDate = DateTime.UtcNow;
        Notes = $"Cancelled: {reason}";

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            AppUserId,
            OrderStatus.Cancelled,
            AppUser.Email,
            AppUser.PhoneNumber));
    }
}