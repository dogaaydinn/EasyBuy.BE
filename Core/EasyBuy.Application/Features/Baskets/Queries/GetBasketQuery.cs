using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Baskets;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Queries;

/// <summary>
/// Query to get user's basket from Redis.
/// </summary>
public sealed class GetBasketQuery : IRequest<Result<BasketDto>>
{
    public Guid UserId { get; set; } // Set from current user context
}
