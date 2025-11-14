using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Handler for UpdateBasketItemCommand.
/// Updates item quantity in Redis basket with stock validation.
/// </summary>
public sealed class UpdateBasketItemCommandHandler : IRequestHandler<UpdateBasketItemCommand, Result<BasketDto>>
{
    private readonly IBasketService _basketService;
    private readonly ILogger<UpdateBasketItemCommandHandler> _logger;

    public UpdateBasketItemCommandHandler(
        IBasketService basketService,
        ILogger<UpdateBasketItemCommandHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating basket item: User={UserId}, ItemId={BasketItemId}, Quantity={Quantity}",
            request.UserId, request.BasketItemId, request.Quantity);

        try
        {
            var basket = await _basketService.UpdateBasketItemAsync(
                request.UserId,
                request.BasketItemId,
                request.Quantity,
                cancellationToken);

            _logger.LogInformation("Successfully updated basket item: User={UserId}, Total items={ItemCount}",
                request.UserId, basket.ItemCount);

            return Result<BasketDto>.Success(basket);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update basket item: User={UserId}, ItemId={BasketItemId}",
                request.UserId, request.BasketItemId);
            return Result<BasketDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket item: User={UserId}, ItemId={BasketItemId}",
                request.UserId, request.BasketItemId);
            return Result<BasketDto>.Failure($"Failed to update basket item: {ex.Message}");
        }
    }
}
