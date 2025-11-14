using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Reviews;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Query to get all reviews for a specific product.
/// Includes average rating calculation.
/// </summary>
public sealed class GetProductReviewsQuery : IRequest<Result<ProductReviewsResult>>
{
    public Guid ProductId { get; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public GetProductReviewsQuery(Guid productId)
    {
        ProductId = productId;
    }
}

/// <summary>
/// Result containing reviews and average rating for a product.
/// </summary>
public sealed class ProductReviewsResult
{
    public PagedResult<ReviewDto> Reviews { get; set; } = null!;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
