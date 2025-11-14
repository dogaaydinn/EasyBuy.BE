namespace EasyBuy.Application.DTOs.Orders;

/// <summary>
/// Order Data Transfer Object for API responses.
/// </summary>
public sealed class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public OrderAddressDto ShippingAddress { get; set; } = new();
    public OrderAddressDto BillingAddress { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}

public sealed class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ProductImageUrl { get; set; }
}

public sealed class OrderAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class CreateOrderDto
{
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public OrderAddressDto ShippingAddress { get; set; } = new();
    public OrderAddressDto BillingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}
