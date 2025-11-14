using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Repositories.Review;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Handler for UpdateReviewCommand.
/// Updates review and invalidates caches.
/// </summary>
public sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly IReviewWriteRepository _writeRepository;
    private readonly IReviewReadRepository _readRepository;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(
        IReviewWriteRepository writeRepository,
        IReviewReadRepository readRepository,
        ILayeredCacheService cache,
        ILogger<UpdateReviewCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating review: {ReviewId}, User: {UserId}", request.ReviewId, request.UserId);

        try
        {
            // Get existing review
            var review = await _readRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return Result<bool>.Failure($"Review not found: {request.ReviewId}");
            }

            // Verify ownership (users can only update their own reviews)
            if (review.UserId != request.UserId)
            {
                return Result<bool>.Failure("You can only update your own reviews");
            }

            // Update review
            review.Rating = request.Rating;
            review.Title = request.Title;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _writeRepository.UpdateAsync(review);

            _logger.LogInformation("Review updated successfully: {ReviewId}", request.ReviewId);

            // Invalidate caches
            await _cache.RemoveAsync($"review:{request.ReviewId}", cancellationToken);
            await _cache.RemoveAsync($"product:{review.ProductId}:reviews", cancellationToken);
            await _cache.RemoveAsync($"product:{review.ProductId}:rating", cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review: {ReviewId}", request.ReviewId);
            return Result<bool>.Failure($"Failed to update review: {ex.Message}");
        }
    }
}
