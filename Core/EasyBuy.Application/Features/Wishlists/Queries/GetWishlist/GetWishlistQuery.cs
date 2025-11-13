using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using MediatR;

namespace EasyBuy.Application.Features.Wishlists.Queries.GetWishlist;

public class GetWishlistQuery : IRequest<Result<List<WishlistItemDto>>>
{
    // Retrieves current user's wishlist
}
