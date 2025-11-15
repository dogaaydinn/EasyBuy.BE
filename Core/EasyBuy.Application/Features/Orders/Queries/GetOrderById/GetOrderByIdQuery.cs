using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Orders.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; init; }
}
