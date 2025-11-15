using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Orders.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Command to create a new order from user's basket
/// </summary>
public sealed record CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public List<CreateOrderItemDto> Items { get; init; } = new();
    public Guid DeliveryMethodId { get; init; }
    public string? CouponCode { get; init; }
    public AddressDto ShippingAddress { get; init; } = new();
    public string? Notes { get; init; }
}
