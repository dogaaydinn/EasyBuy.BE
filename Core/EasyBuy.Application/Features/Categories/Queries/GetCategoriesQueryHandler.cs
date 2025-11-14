using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Categories;
using EasyBuy.Application.Repositories.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Handler for GetCategoriesQuery.
/// Retrieves categories with pagination, filtering, and sorting.
/// Uses multi-level caching for performance.
/// </summary>
public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<PagedResult<CategoryDto>>>
{
    private readonly ICategoryReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        ICategoryReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting categories: Page={Page}, Size={Size}, ParentId={ParentId}, IsActive={IsActive}",
            request.PageNumber, request.PageSize, request.ParentCategoryId, request.IsActive);

        try
        {
            // Build cache key based on query parameters
            var cacheKey = $"categories:list:{request.PageNumber}:{request.PageSize}:{request.ParentCategoryId}:{request.IsActive}:{request.OrderBy}:{request.Descending}";

            var pagedResult = await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    // Get all categories
                    var categories = await _repository.GetAllAsync();

                    // Apply filters
                    if (request.ParentCategoryId.HasValue)
                    {
                        categories = categories.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
                    }

                    if (request.IsActive.HasValue)
                    {
                        categories = categories.Where(c => c.IsActive == request.IsActive.Value);
                    }

                    // Apply sorting
                    categories = request.OrderBy?.ToLower() switch
                    {
                        "name" => request.Descending
                            ? categories.OrderByDescending(c => c.Name)
                            : categories.OrderBy(c => c.Name),
                        "createdat" => request.Descending
                            ? categories.OrderByDescending(c => c.CreatedAt)
                            : categories.OrderBy(c => c.CreatedAt),
                        _ => request.Descending
                            ? categories.OrderByDescending(c => c.DisplayOrder).ThenByDescending(c => c.Name)
                            : categories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                    };

                    // Get total count
                    var totalCount = categories.Count();

                    // Apply pagination
                    var pagedCategories = categories
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList();

                    // Map to DTOs with product count
                    var categoryDtos = _mapper.Map<List<CategoryDto>>(pagedCategories);
                    
                    // Enrich with product and subcategory counts
                    foreach (var dto in categoryDtos)
                    {
                        var category = pagedCategories.First(c => c.Id == dto.Id);
                        dto.ProductCount = category.Products.Count;
                        dto.SubCategoryCount = category.SubCategories.Count;
                    }

                    return new PagedResult<CategoryDto>(
                        categoryDtos,
                        totalCount,
                        request.PageNumber,
                        request.PageSize);
                },
                TimeSpan.FromMinutes(10),
                cancellationToken);

            _logger.LogInformation("Retrieved {Count} categories out of {Total}",
                pagedResult.Items.Count, pagedResult.TotalCount);

            return Result<PagedResult<CategoryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Result<PagedResult<CategoryDto>>.Failure($"Failed to retrieve categories: {ex.Message}");
        }
    }
}
