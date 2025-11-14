using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Reviews;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Queries;

/// <summary>
/// Query to get all reviews with pagination and filtering.
/// Supports filtering by product, user, and rating.
/// </summary>
public sealed class GetReviewsQuery : IRequest<Result<PagedResult<ReviewDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ProductId { get; set; }
    public Guid? UserId { get; set; }
    public int? MinRating { get; set; }
    public bool? IsApproved { get; set; }
    public string? OrderBy { get; set; } = "ReviewDate";
    public bool Descending { get; set; } = true;
}
