using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Manager");

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
            {
                return Result<bool>.Failure("Review not found");
            }

            // Users can delete their own reviews, admins can delete any
            if (!isAdmin && review.UserId != userId)
            {
                return Result<bool>.Failure("You can only delete your own reviews");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} deleted by user {UserId}", request.ReviewId, userId);

            return Result<bool>.Success(true, "Review deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", request.ReviewId);
            return Result<bool>.Failure($"An error occurred while deleting the review: {ex.Message}");
        }
    }
}
