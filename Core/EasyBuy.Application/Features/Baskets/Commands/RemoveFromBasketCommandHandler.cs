using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Handler for RemoveFromBasketCommand.
/// Removes item from Redis basket.
/// </summary>
public sealed class RemoveFromBasketCommandHandler : IRequestHandler<RemoveFromBasketCommand, Result<BasketDto>>
{
    private readonly IBasketService _basketService;
    private readonly ILogger<RemoveFromBasketCommandHandler> _logger;

    public RemoveFromBasketCommandHandler(
        IBasketService basketService,
        ILogger<RemoveFromBasketCommandHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(RemoveFromBasketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing from basket: User={UserId}, ItemId={BasketItemId}",
            request.UserId, request.BasketItemId);

        try
        {
            var basket = await _basketService.RemoveFromBasketAsync(
                request.UserId,
                request.BasketItemId,
                cancellationToken);

            _logger.LogInformation("Successfully removed from basket: User={UserId}, Remaining items={ItemCount}",
                request.UserId, basket.ItemCount);

            return Result<BasketDto>.Success(basket);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to remove from basket: User={UserId}, ItemId={BasketItemId}",
                request.UserId, request.BasketItemId);
            return Result<BasketDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from basket: User={UserId}, ItemId={BasketItemId}",
                request.UserId, request.BasketItemId);
            return Result<BasketDto>.Failure($"Failed to remove from basket: {ex.Message}");
        }
    }
}
