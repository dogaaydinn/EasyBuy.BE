using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Handler for AddToBasketCommand.
/// Adds item to Redis basket with stock validation.
/// </summary>
public sealed class AddToBasketCommandHandler : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    private readonly IBasketService _basketService;
    private readonly ILogger<AddToBasketCommandHandler> _logger;

    public AddToBasketCommandHandler(
        IBasketService basketService,
        ILogger<AddToBasketCommandHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding to basket: User={UserId}, Product={ProductId}, Quantity={Quantity}",
            request.UserId, request.ProductId, request.Quantity);

        try
        {
            var basket = await _basketService.AddToBasketAsync(
                request.UserId,
                request.ProductId,
                request.Quantity,
                cancellationToken);

            _logger.LogInformation("Successfully added to basket: User={UserId}, Total items={ItemCount}",
                request.UserId, basket.ItemCount);

            return Result<BasketDto>.Success(basket);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to add to basket: User={UserId}, Product={ProductId}",
                request.UserId, request.ProductId);
            return Result<BasketDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to basket: User={UserId}, Product={ProductId}",
                request.UserId, request.ProductId);
            return Result<BasketDto>.Failure($"Failed to add to basket: {ex.Message}");
        }
    }
}
