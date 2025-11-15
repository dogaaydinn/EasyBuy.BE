using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

/// <summary>
/// Represents an item in an order (Order-Product many-to-many relationship with additional properties).
/// This is essential for tracking individual items within an order with their specific quantity and price.
/// </summary>
public class OrderItem : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Order Order { get; set; }

    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }

    /// <summary>
    /// Quantity of this specific product in the order
    /// </summary>
    public required int Quantity { get; set; }

    /// <summary>
    /// Unit price at the time of order (important for historical accuracy)
    /// </summary>
    public required decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line item (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    /// <summary>
    /// Product name at time of order (for historical records if product is deleted/renamed)
    /// </summary>
    public string? ProductName { get; set; }
}
