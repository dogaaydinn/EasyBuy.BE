using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? Notes { get; init; }
}
