using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.ClearBasket;

public class ClearBasketCommand : IRequest<Result<bool>>
{
    // No properties needed - uses current user's basket
}
