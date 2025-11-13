using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly EasyBuyDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(
        EasyBuyDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateReviewCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
            {
                return Result<bool>.Failure("Review not found");
            }

            // Users can only update their own reviews
            if (review.UserId != userId)
            {
                return Result<bool>.Failure("You can only update your own reviews");
            }

            review.Rating = request.Rating;
            review.Title = request.Title;
            review.Comment = request.Comment;
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} updated by user {UserId}", request.ReviewId, userId);

            return Result<bool>.Success(true, "Review updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", request.ReviewId);
            return Result<bool>.Failure($"An error occurred while updating the review: {ex.Message}");
        }
    }
}
