using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Queries.GetBasket;

public class GetBasketQuery : IRequest<Result<BasketDto>>
{
    // No properties needed - retrieves current user's basket
}
