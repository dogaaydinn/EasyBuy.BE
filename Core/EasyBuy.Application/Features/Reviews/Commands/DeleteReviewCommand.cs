using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Command to delete a review.
/// Users can only delete their own reviews, admins can delete any review.
/// </summary>
public sealed class DeleteReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; } // Set from current user context
    public bool IsAdmin { get; set; } // Set from current user context
}
