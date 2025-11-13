using EasyBuy.Application.Common.Models;
using EasyBuy.Domain.Enums;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Result<bool>>
{
    public required Guid OrderId { get; set; }
    public required OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public string? TrackingNumber { get; set; }
}
