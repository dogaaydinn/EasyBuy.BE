using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Command to update an existing review.
/// Users can only update their own reviews.
/// </summary>
public sealed class UpdateReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; } // Set from current user context
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
