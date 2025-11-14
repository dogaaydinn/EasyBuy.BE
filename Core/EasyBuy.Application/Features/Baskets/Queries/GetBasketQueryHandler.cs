using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Basket;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Queries;

/// <summary>
/// Handler for GetBasketQuery.
/// Retrieves user's basket from Redis.
/// </summary>
public sealed class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    private readonly IBasketService _basketService;
    private readonly ILogger<GetBasketQueryHandler> _logger;

    public GetBasketQueryHandler(
        IBasketService basketService,
        ILogger<GetBasketQueryHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting basket: User={UserId}", request.UserId);

        try
        {
            var basket = await _basketService.GetBasketAsync(request.UserId, cancellationToken);

            if (basket == null)
            {
                // Return empty basket instead of failure
                basket = new BasketDto
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Items = new List<BasketItemDto>()
                };
                
                _logger.LogInformation("No basket found for user, returning empty basket: User={UserId}", request.UserId);
            }
            else
            {
                _logger.LogInformation("Retrieved basket: User={UserId}, Items={ItemCount}, Total={Total}",
                    request.UserId, basket.ItemCount, basket.Total);
            }

            return Result<BasketDto>.Success(basket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket: User={UserId}", request.UserId);
            return Result<BasketDto>.Failure($"Failed to retrieve basket: {ex.Message}");
        }
    }
}
