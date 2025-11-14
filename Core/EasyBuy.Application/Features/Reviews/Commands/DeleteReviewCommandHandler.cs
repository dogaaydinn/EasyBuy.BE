using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Repositories.Review;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Handler for DeleteReviewCommand.
/// Deletes review and invalidates caches.
/// </summary>
public sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result<bool>>
{
    private readonly IReviewWriteRepository _writeRepository;
    private readonly IReviewReadRepository _readRepository;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IReviewWriteRepository writeRepository,
        IReviewReadRepository readRepository,
        ILayeredCacheService cache,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting review: {ReviewId}, User: {UserId}, IsAdmin: {IsAdmin}",
            request.ReviewId, request.UserId, request.IsAdmin);

        try
        {
            // Get review
            var review = await _readRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return Result<bool>.Failure($"Review not found: {request.ReviewId}");
            }

            // Verify ownership or admin rights
            if (!request.IsAdmin && review.UserId != request.UserId)
            {
                return Result<bool>.Failure("You can only delete your own reviews");
            }

            var productId = review.ProductId;

            // Delete review
            await _writeRepository.DeleteAsync(review);

            _logger.LogInformation("Review deleted successfully: {ReviewId}", request.ReviewId);

            // Invalidate caches
            await _cache.RemoveAsync($"review:{request.ReviewId}", cancellationToken);
            await _cache.RemoveAsync($"product:{productId}:reviews", cancellationToken);
            await _cache.RemoveAsync($"product:{productId}:rating", cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review: {ReviewId}", request.ReviewId);
            return Result<bool>.Failure($"Failed to delete review: {ex.Message}");
        }
    }
}
