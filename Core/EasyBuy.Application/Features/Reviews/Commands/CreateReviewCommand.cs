using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Command to create a new review.
/// User can only review products once.
/// </summary>
public sealed class CreateReviewCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; } // Set from current user context
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
