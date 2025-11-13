using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Wishlists.Queries.GetWishlist;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, Result<List<WishlistItemDto>>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWishlistQueryHandler> _logger;

    public GetWishlistQueryHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetWishlistQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<WishlistItemDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<List<WishlistItemDto>>.Failure("User not authenticated");
            }

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImageFiles)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedDate)
                .ProjectTo<WishlistItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<WishlistItemDto>>.Success(wishlistItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist for user {UserId}", _currentUserService.UserId);
            return Result<List<WishlistItemDto>>.Failure($"An error occurred: {ex.Message}");
        }
    }
}
