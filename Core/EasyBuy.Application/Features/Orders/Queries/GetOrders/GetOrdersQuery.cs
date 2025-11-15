using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Orders.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<Result<PagedResult<OrderDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }
    public string? UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
