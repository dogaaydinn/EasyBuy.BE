using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Domain.Enums;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQuery : IRequest<Result<PagedResult<OrderListDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "OrderDate";
    public bool SortDescending { get; set; } = true;
    public Guid? UserId { get; set; } // For admin to filter by user
}
