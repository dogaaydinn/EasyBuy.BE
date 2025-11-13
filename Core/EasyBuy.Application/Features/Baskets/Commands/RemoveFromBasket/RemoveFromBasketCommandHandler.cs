using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.RemoveFromBasket;

public class RemoveFromBasketCommandHandler : IRequestHandler<RemoveFromBasketCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveFromBasketCommandHandler> _logger;

    public RemoveFromBasketCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<RemoveFromBasketCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemoveFromBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == request.BasketItemId && bi.Basket.UserId == userId, cancellationToken);

            if (basketItem == null)
            {
                return Result<bool>.Failure("Basket item not found");
            }

            _context.BasketItems.Remove(basketItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed basket item {BasketItemId} from user {UserId} basket",
                request.BasketItemId, userId);

            return Result<bool>.Success(true, "Item removed from basket");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing basket item {BasketItemId}", request.BasketItemId);
            return Result<bool>.Failure($"An error occurred while removing item from basket: {ex.Message}");
        }
    }
}
