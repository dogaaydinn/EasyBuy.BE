using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Command to add item to basket or update quantity if already exists.
/// </summary>
public sealed class AddToBasketCommand : IRequest<Result<BasketDto>>
{
    public Guid UserId { get; set; } // Set from current user context
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
