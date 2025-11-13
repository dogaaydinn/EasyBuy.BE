using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Reviews.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly EasyBuyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductReviewsQueryHandler> _logger;

    public GetProductReviewsQueryHandler(
        EasyBuyDbContext context,
        IMapper mapper,
        ILogger<GetProductReviewsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == request.ProductId)
                .AsQueryable();

            // Filter by rating if specified
            if (request.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == request.Rating.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "rating" => request.SortDescending
                    ? query.OrderByDescending(r => r.Rating)
                    : query.OrderBy(r => r.Rating),
                "helpful" => request.SortDescending
                    ? query.OrderByDescending(r => r.HelpfulCount)
                    : query.OrderBy(r => r.HelpfulCount),
                _ => request.SortDescending
                    ? query.OrderByDescending(r => r.CreatedDate)
                    : query.OrderBy(r => r.CreatedDate)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var reviews = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<ReviewDto>
            {
                Items = reviews,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<ReviewDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", request.ProductId);
            return Result<PagedResult<ReviewDto>>.Failure($"An error occurred while retrieving reviews: {ex.Message}");
        }
    }
}
