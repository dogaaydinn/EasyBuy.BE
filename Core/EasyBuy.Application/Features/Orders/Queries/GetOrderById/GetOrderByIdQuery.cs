using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public required Guid Id { get; set; }
}
