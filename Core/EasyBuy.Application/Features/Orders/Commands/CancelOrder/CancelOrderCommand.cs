using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommand : IRequest<Result<bool>>
{
    public required Guid OrderId { get; set; }
    public string? Reason { get; set; }
}
