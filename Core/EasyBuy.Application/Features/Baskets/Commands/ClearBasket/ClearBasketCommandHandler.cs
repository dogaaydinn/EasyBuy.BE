using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.ClearBasket;

public class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ClearBasketCommandHandler> _logger;

    public ClearBasketCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ClearBasketCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var basket = await _context.Baskets
                .Include(b => b.BasketItems)
                .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

            if (basket == null)
            {
                return Result<bool>.Success(true, "Basket is already empty");
            }

            var itemCount = basket.BasketItems.Count;
            _context.BasketItems.RemoveRange(basket.BasketItems);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleared basket for user {UserId}. Removed {ItemCount} items",
                userId, itemCount);

            return Result<bool>.Success(true, "Basket cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user {UserId}", _currentUserService.UserId);
            return Result<bool>.Failure($"An error occurred while clearing basket: {ex.Message}");
        }
    }
}
