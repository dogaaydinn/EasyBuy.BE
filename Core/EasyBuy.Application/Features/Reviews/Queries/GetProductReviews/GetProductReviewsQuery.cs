using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using MediatR;

namespace EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;

public class GetProductReviewsQuery : IRequest<Result<PagedResult<ReviewDto>>>
{
    public required Guid ProductId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Rating { get; set; }
    public string? SortBy { get; set; } = "CreatedDate";
    public bool SortDescending { get; set; } = true;
}
