using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands;

/// <summary>
/// Command to clear entire basket.
/// </summary>
public sealed class ClearBasketCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; } // Set from current user context
}
