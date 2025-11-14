using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Reviews;
using EasyBuy.Application.Repositories.Review;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Handler for GetProductReviewsQuery.
/// Retrieves all reviews for a product with average rating and distribution.
/// </summary>
public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, Result<ProductReviewsResult>>
{
    private readonly IReviewReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetProductReviewsQueryHandler> _logger;

    public GetProductReviewsQueryHandler(
        IReviewReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetProductReviewsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<ProductReviewsResult>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reviews for product: {ProductId}, Page={Page}, Size={Size}",
            request.ProductId, request.PageNumber, request.PageSize);

        try
        {
            var cacheKey = $"product:{request.ProductId}:reviews:{request.PageNumber}:{request.PageSize}";

            var result = await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    // Get all reviews for the product
                    var allReviews = (await _repository.GetProductReviewsAsync(request.ProductId)).ToList();
                    var totalReviews = allReviews.Count;

                    // Calculate average rating
                    var averageRating = totalReviews > 0 
                        ? allReviews.Average(r => r.Rating) 
                        : 0;

                    // Calculate rating distribution
                    var ratingDistribution = new Dictionary<int, int>
                    {
                        { 5, allReviews.Count(r => r.Rating == 5) },
                        { 4, allReviews.Count(r => r.Rating == 4) },
                        { 3, allReviews.Count(r => r.Rating == 3) },
                        { 2, allReviews.Count(r => r.Rating == 2) },
                        { 1, allReviews.Count(r => r.Rating == 1) }
                    };

                    // Apply pagination
                    var pagedReviews = allReviews
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList();

                    // Map to DTOs
                    var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews);

                    var pagedResult = new PagedResult<ReviewDto>(
                        reviewDtos,
                        totalReviews,
                        request.PageNumber,
                        request.PageSize);

                    return new ProductReviewsResult
                    {
                        Reviews = pagedResult,
                        AverageRating = Math.Round(averageRating, 2),
                        TotalReviews = totalReviews,
                        RatingDistribution = ratingDistribution
                    };
                },
                TimeSpan.FromMinutes(10),
                cancellationToken);

            _logger.LogInformation("Retrieved {Count} reviews for product: {ProductId}, Avg Rating: {AvgRating}",
                result.Reviews.Items.Count, request.ProductId, result.AverageRating);

            return Result<ProductReviewsResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product reviews: {ProductId}", request.ProductId);
            return Result<ProductReviewsResult>.Failure($"Failed to retrieve product reviews: {ex.Message}");
        }
    }
}
