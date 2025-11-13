using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.UpdateBasketItemQuantity;

public class UpdateBasketItemQuantityCommand : IRequest<Result<bool>>
{
    public required Guid BasketItemId { get; set; }
    public required int Quantity { get; set; }
}
