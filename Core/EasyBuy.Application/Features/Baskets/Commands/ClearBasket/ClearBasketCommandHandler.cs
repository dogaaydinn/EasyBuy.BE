using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.ClearBasket;

public sealed class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, Result>
{
    private readonly IWriteRepository<Basket> _basketWriteRepository;
    private readonly IReadRepository<Basket> _basketReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ClearBasketCommandHandler> _logger;

    public ClearBasketCommandHandler(
        IWriteRepository<Basket> basketWriteRepository,
        IReadRepository<Basket> basketReadRepository,
        ICurrentUserService currentUserService,
        ILogger<ClearBasketCommandHandler> logger)
    {
        _basketWriteRepository = basketWriteRepository;
        _basketReadRepository = basketReadRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Result.Failure("User not authenticated");
            }

            var baskets = await _basketReadRepository.GetAllAsync();
            var basket = baskets.FirstOrDefault(b => b.AppUserId == userId);

            if (basket == null)
            {
                return Result.Success(); // Already empty
            }

            basket.BasketItems.Clear();
            await _basketWriteRepository.UpdateAsync(basket);

            _logger.LogInformation("Cleared basket for user {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket");
            return Result.Failure($"Failed to clear basket: {ex.Message}");
        }
    }
}
