using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.AddToBasket;

public class AddToBasketCommand : IRequest<Result<BasketDto>>
{
    public required Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
