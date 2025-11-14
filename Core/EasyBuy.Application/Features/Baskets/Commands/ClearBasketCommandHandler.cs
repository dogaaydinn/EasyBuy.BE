using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Basket;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Handler for ClearBasketCommand.
/// Clears all items from Redis basket.
/// </summary>
public sealed class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, Result<bool>>
{
    private readonly IBasketService _basketService;
    private readonly ILogger<ClearBasketCommandHandler> _logger;

    public ClearBasketCommandHandler(
        IBasketService basketService,
        ILogger<ClearBasketCommandHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing basket: User={UserId}", request.UserId);

        try
        {
            await _basketService.ClearBasketAsync(request.UserId, cancellationToken);

            _logger.LogInformation("Successfully cleared basket: User={UserId}", request.UserId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket: User={UserId}", request.UserId);
            return Result<bool>.Failure($"Failed to clear basket: {ex.Message}");
        }
    }
}
