using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;

public class RemoveFromBasketCommand : IRequest<Result<bool>>
{
    public required Guid BasketItemId { get; set; }
}
