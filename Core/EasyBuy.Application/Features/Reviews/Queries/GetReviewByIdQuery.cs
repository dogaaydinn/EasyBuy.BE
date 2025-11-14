using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Reviews;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Query to get a single review by ID.
/// </summary>
public sealed class GetReviewByIdQuery : IRequest<Result<ReviewDto>>
{
    public Guid ReviewId { get; }

    public GetReviewByIdQuery(Guid reviewId)
    {
        ReviewId = reviewId;
    }
}
