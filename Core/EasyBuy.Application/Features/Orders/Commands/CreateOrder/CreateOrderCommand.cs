using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Domain.Enums;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public required List<OrderItemDto> Items { get; set; }
    public Guid? AddressId { get; set; }
    public string? CouponCode { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? PaymentDetails { get; set; }
    public string? Notes { get; set; }
}
