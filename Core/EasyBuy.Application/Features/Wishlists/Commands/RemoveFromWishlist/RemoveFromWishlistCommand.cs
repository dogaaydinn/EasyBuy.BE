using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommand : IRequest<Result<bool>>
{
    public required Guid WishlistId { get; set; }
}
