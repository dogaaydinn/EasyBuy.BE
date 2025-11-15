using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Baskets.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;

public sealed record RemoveFromBasketCommand : IRequest<Result<BasketDto>>
{
    public Guid ProductId { get; init; }
}
