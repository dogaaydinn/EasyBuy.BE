using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveFromWishlistCommandHandler> _logger;

    public RemoveFromWishlistCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<RemoveFromWishlistCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == request.WishlistId && w.UserId == userId, cancellationToken);

            if (wishlistItem == null)
            {
                return Result<bool>.Failure("Wishlist item not found");
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wishlist item {WishlistId} removed for user {UserId}", request.WishlistId, userId);

            return Result<bool>.Success(true, "Product removed from wishlist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing wishlist item {WishlistId}", request.WishlistId);
            return Result<bool>.Failure($"An error occurred: {ex.Message}");
        }
    }
}
