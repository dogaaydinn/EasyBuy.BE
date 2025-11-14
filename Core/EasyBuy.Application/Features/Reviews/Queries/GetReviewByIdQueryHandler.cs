using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Reviews;
using EasyBuy.Application.Repositories.Review;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Handler for GetReviewByIdQuery.
/// Retrieves single review with caching.
/// </summary>
public sealed class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    private readonly IReviewReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetReviewByIdQueryHandler> _logger;

    public GetReviewByIdQueryHandler(
        IReviewReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetReviewByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting review: {ReviewId}", request.ReviewId);

        try
        {
            var reviewDto = await _cache.GetOrSetAsync(
                $"review:{request.ReviewId}",
                async () =>
                {
                    var review = await _repository.GetByIdAsync(request.ReviewId);
                    if (review == null)
                    {
                        return null;
                    }

                    return _mapper.Map<ReviewDto>(review);
                },
                TimeSpan.FromMinutes(15),
                cancellationToken);

            if (reviewDto == null)
            {
                return Result<ReviewDto>.Failure($"Review not found: {request.ReviewId}");
            }

            _logger.LogInformation("Retrieved review: {ReviewId}, Product: {ProductId}, Rating: {Rating}",
                reviewDto.Id, reviewDto.ProductId, reviewDto.Rating);

            return Result<ReviewDto>.Success(reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review: {ReviewId}", request.ReviewId);
            return Result<ReviewDto>.Failure($"Failed to retrieve review: {ex.Message}");
        }
    }
}
