using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Reviews.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery : IRequest<Result<List<ReviewDto>>>
{
    public Guid ProductId { get; init; }
    public int? MinRating { get; init; }
    public bool? VerifiedOnly { get; init; }
}
