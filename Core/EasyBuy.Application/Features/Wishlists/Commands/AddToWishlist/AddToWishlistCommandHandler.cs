using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddToWishlistCommandHandler> _logger;

    public AddToWishlistCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AddToWishlistCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            // Check if product exists
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (product == null)
            {
                return Result<bool>.Failure("Product not found");
            }

            // Check if already in wishlist
            var exists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == request.ProductId, cancellationToken);

            if (exists)
            {
                return Result<bool>.Failure("Product is already in your wishlist");
            }

            // Add to wishlist
            var wishlistItem = new Wishlist
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Wishlists.AddAsync(wishlistItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product {ProductId} added to wishlist for user {UserId}", request.ProductId, userId);

            return Result<bool>.Success(true, "Product added to wishlist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to wishlist", request.ProductId);
            return Result<bool>.Failure($"An error occurred: {ex.Message}");
        }
    }
}
