using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Baskets.Commands.UpdateBasketItemQuantity;

public class UpdateBasketItemQuantityCommandHandler : IRequestHandler<UpdateBasketItemQuantityCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateBasketItemQuantityCommandHandler> _logger;

    public UpdateBasketItemQuantityCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateBasketItemQuantityCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateBasketItemQuantityCommand request, CancellationToken cancellationToken)
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
                .Include(bi => bi.Product)
                .FirstOrDefaultAsync(bi => bi.Id == request.BasketItemId && bi.Basket.UserId == userId, cancellationToken);

            if (basketItem == null)
            {
                return Result<bool>.Failure("Basket item not found");
            }

            // Check stock availability
            if (basketItem.Product.Stock < request.Quantity)
            {
                return Result<bool>.Failure($"Insufficient stock. Available: {basketItem.Product.Stock}, Requested: {request.Quantity}");
            }

            var oldQuantity = basketItem.Quantity;
            basketItem.Quantity = request.Quantity;
            basketItem.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated basket item {BasketItemId} quantity from {OldQuantity} to {NewQuantity}",
                request.BasketItemId, oldQuantity, request.Quantity);

            return Result<bool>.Success(true, "Basket item quantity updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket item {BasketItemId} quantity", request.BasketItemId);
            return Result<bool>.Failure($"An error occurred while updating basket item quantity: {ex.Message}");
        }
    }
}
