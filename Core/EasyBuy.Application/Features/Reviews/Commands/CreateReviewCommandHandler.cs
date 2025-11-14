using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Application.Repositories.Review;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Commands;

/// <summary>
/// Handler for CreateReviewCommand.
/// Creates review, updates product rating cache, and checks for duplicate reviews.
/// </summary>
public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IReviewWriteRepository _writeRepository;
    private readonly IReviewReadRepository _readRepository;
    private readonly IProductReadRepository _productRepository;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IReviewWriteRepository writeRepository,
        IReviewReadRepository readRepository,
        IProductReadRepository productRepository,
        ILayeredCacheService cache,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating review for product: {ProductId}, User: {UserId}, Rating: {Rating}",
            request.ProductId, request.UserId, request.Rating);

        try
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return Result<Guid>.Failure($"Product not found: {request.ProductId}");
            }

            // Check if user has already reviewed this product
            var hasReviewed = await _readRepository.HasUserReviewedProductAsync(request.UserId, request.ProductId);
            if (hasReviewed)
            {
                return Result<Guid>.Failure("You have already reviewed this product. You can update your existing review instead.");
            }

            // Create review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Product = product,
                UserId = request.UserId,
                // User will be loaded by EF Core
                User = null!, // Required property set to null! to satisfy compiler
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                ReviewDate = DateTime.UtcNow,
                IsVerifiedPurchase = false, // TODO: Check if user actually purchased the product
                HelpfulCount = 0,
                IsApproved = true // Auto-approve for now, can add moderation later
            };

            await _writeRepository.AddAsync(review);

            _logger.LogInformation("Review created successfully: {ReviewId}, Product: {ProductId}, Rating: {Rating}",
                review.Id, review.ProductId, review.Rating);

            // Invalidate product cache to reflect new review
            await _cache.RemoveAsync($"product:{request.ProductId}", cancellationToken);
            await _cache.RemoveAsync($"product:{request.ProductId}:reviews", cancellationToken);
            await _cache.RemoveAsync($"product:{request.ProductId}:rating", cancellationToken);

            return Result<Guid>.Success(review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for product: {ProductId}", request.ProductId);
            return Result<Guid>.Failure($"Failed to create review: {ex.Message}");
        }
    }
}
