using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands;

/// <summary>
/// Command to cancel an order.
/// Only orders in 'Created' or 'Processing' status can be cancelled.
/// </summary>
public sealed class CancelOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string? CancellationReason { get; set; }
}
