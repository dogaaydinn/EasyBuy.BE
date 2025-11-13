using EasyBuy.Domain.Enums;

namespace EasyBuy.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DeliveryDto? Delivery { get; set; }
    public PaymentDto? Payment { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
}

public class OrderListDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
}

public class CreateOrderDto
{
    public required List<OrderItemDto> Items { get; set; }
    public Guid? AddressId { get; set; }
    public string? CouponCode { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? PaymentDetails { get; set; }
}

public class UpdateOrderStatusDto
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}

public class DeliveryDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
