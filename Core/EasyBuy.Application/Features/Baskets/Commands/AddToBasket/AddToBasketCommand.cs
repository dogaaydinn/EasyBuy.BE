using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Baskets.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.AddToBasket;

public sealed record AddToBasketCommand : IRequest<Result<BasketDto>>
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; } = 1;
}
