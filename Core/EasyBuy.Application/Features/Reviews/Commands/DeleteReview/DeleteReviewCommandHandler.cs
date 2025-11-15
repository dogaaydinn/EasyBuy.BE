using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands.DeleteReview;

public sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IWriteRepository<Review> _reviewWriteRepository;
    private readonly IReadRepository<Review> _reviewReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IWriteRepository<Review> reviewWriteRepository,
        IReadRepository<Review> reviewReadRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _reviewWriteRepository = reviewWriteRepository;
        _reviewReadRepository = reviewReadRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var review = await _reviewReadRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return Result.Failure($"Review {request.ReviewId} not found");
            }

            // Check if user owns the review or is admin
            var userId = _currentUserService.UserId;
            if (review.AppUserId != userId && !_currentUserService.IsInRole("Admin"))
            {
                return Result.Failure("You do not have permission to delete this review");
            }

            await _reviewWriteRepository.DeleteAsync(review);

            _logger.LogInformation(
                "Review {ReviewId} deleted by user {UserId}",
                request.ReviewId, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", request.ReviewId);
            return Result.Failure($"Failed to delete review: {ex.Message}");
        }
    }
}
