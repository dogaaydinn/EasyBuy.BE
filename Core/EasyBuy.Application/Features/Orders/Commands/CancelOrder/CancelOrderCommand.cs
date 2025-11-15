using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
