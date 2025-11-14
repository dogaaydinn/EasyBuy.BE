using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries;

/// <summary>
/// Query to get a specific order by ID.
/// Includes all order items and address information.
/// </summary>
public sealed class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }

    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
