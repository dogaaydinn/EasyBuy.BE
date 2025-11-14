using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Command to remove item from basket.
/// </summary>
public sealed class RemoveFromBasketCommand : IRequest<Result<BasketDto>>
{
    public Guid UserId { get; set; } // Set from current user context
    public Guid BasketItemId { get; set; }
}
