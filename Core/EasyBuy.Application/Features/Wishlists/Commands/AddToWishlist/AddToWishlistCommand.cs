using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistCommand : IRequest<Result<bool>>
{
    public required Guid ProductId { get; set; }
}
