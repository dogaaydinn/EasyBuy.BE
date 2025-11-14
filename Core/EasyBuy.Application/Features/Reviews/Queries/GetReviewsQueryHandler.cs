using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Reviews;
using EasyBuy.Application.Repositories.Review;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Handler for GetReviewsQuery.
/// Retrieves reviews with pagination, filtering, and sorting.
/// </summary>
public sealed class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IReviewReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetReviewsQueryHandler> _logger;

    public GetReviewsQueryHandler(
        IReviewReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetReviewsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reviews: Page={Page}, Size={Size}, ProductId={ProductId}, UserId={UserId}",
            request.PageNumber, request.PageSize, request.ProductId, request.UserId);

        try
        {
            // Get all reviews
            var reviews = await _repository.GetAllAsync();

            // Apply filters
            if (request.ProductId.HasValue)
            {
                reviews = reviews.Where(r => r.ProductId == request.ProductId.Value);
            }

            if (request.UserId.HasValue)
            {
                reviews = reviews.Where(r => r.UserId == request.UserId.Value);
            }

            if (request.MinRating.HasValue)
            {
                reviews = reviews.Where(r => r.Rating >= request.MinRating.Value);
            }

            if (request.IsApproved.HasValue)
            {
                reviews = reviews.Where(r => r.IsApproved == request.IsApproved.Value);
            }

            // Apply sorting
            reviews = request.OrderBy?.ToLower() switch
            {
                "rating" => request.Descending
                    ? reviews.OrderByDescending(r => r.Rating)
                    : reviews.OrderBy(r => r.Rating),
                "helpfulcount" => request.Descending
                    ? reviews.OrderByDescending(r => r.HelpfulCount)
                    : reviews.OrderBy(r => r.HelpfulCount),
                _ => request.Descending
                    ? reviews.OrderByDescending(r => r.ReviewDate)
                    : reviews.OrderBy(r => r.ReviewDate)
            };

            // Get total count
            var totalCount = reviews.Count();

            // Apply pagination
            var pagedReviews = reviews
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var reviewDtos = _mapper.Map<List<ReviewDto>>(pagedReviews);

            var pagedResult = new PagedResult<ReviewDto>(
                reviewDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} reviews out of {Total}",
                reviewDtos.Count, totalCount);

            return Result<PagedResult<ReviewDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews");
            return Result<PagedResult<ReviewDto>>.Failure($"Failed to retrieve reviews: {ex.Message}");
        }
    }
}
