using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Command to create a new order.
/// Implements CQRS pattern with MediatR.
/// </summary>
public sealed class CreateOrderCommand : IRequest<Result<Guid>>
{
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public OrderAddressDto ShippingAddress { get; set; } = new();
    public OrderAddressDto BillingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}
