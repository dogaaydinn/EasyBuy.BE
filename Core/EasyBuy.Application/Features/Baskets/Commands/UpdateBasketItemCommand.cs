using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Command to update basket item quantity.
/// </summary>
public sealed class UpdateBasketItemCommand : IRequest<Result<BasketDto>>
{
    public Guid UserId { get; set; } // Set from current user context
    public Guid BasketItemId { get; set; }
    public int Quantity { get; set; }
}
