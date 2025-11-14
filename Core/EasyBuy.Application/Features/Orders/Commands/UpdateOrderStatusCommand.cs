using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Command to update order status.
/// Used for order workflow progression (Processing, Shipped, Delivered, Cancelled).
/// </summary>
public sealed class UpdateOrderStatusCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}
