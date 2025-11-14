using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries;

/// <summary>
/// Query to get all orders with pagination and filtering.
/// Supports filtering by status and user.
/// </summary>
public sealed class GetOrdersQuery : IRequest<Result<PagedResult<OrderDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public Guid? UserId { get; set; }
    public string? OrderBy { get; set; } = "OrderDate";
    public bool Descending { get; set; } = true;
}
