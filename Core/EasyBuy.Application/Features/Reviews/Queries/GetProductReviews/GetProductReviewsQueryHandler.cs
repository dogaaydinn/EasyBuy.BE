using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Persistence;
using EasyBuy.Application.Features.Reviews.DTOs;
using EasyBuy.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, Result<List<ReviewDto>>>
{
    private readonly IReadRepository<Review> _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductReviewsQueryHandler> _logger;

    public GetProductReviewsQueryHandler(
        IReadRepository<Review> reviewRepository,
        IMapper mapper,
        ILogger<GetProductReviewsQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var reviews = await _reviewRepository.GetAllAsync();
            var query = reviews.Where(r => r.ProductId == request.ProductId);

            // Apply filters
            if (request.MinRating.HasValue)
            {
                query = query.Where(r => r.Rating >= request.MinRating.Value);
            }

            if (request.VerifiedOnly == true)
            {
                query = query.Where(r => r.IsVerifiedPurchase);
            }

            // Sort by newest first
            var reviewList = query.OrderByDescending(r => r.CreatedDate).ToList();

            var reviewDtos = _mapper.Map<List<ReviewDto>>(reviewList);
            return Result<List<ReviewDto>>.Success(reviewDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", request.ProductId);
            return Result<List<ReviewDto>>.Failure($"Failed to retrieve reviews: {ex.Message}");
        }
    }
}
