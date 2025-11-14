using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Categories;
using EasyBuy.Application.Repositories.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Handler for GetCategoryTreeQuery.
/// Retrieves complete hierarchical category tree with caching.
/// </summary>
public sealed class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetCategoryTreeQueryHandler> _logger;

    public GetCategoryTreeQueryHandler(
        ICategoryReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetCategoryTreeQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category tree, IncludeInactive: {IncludeInactive}", request.IncludeInactive);

        try
        {
            var cacheKey = $"categories:tree:{request.IncludeInactive}";

            var categoryTree = await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    // Get all categories from repository
                    var allCategories = (await _repository.GetAllAsync()).ToList();

                    // Filter inactive if needed
                    if (!request.IncludeInactive)
                    {
                        allCategories = allCategories.Where(c => c.IsActive).ToList();
                    }

                    // Build hierarchical structure
                    var categoryDict = allCategories.ToDictionary(c => c.Id);
                    var rootCategories = allCategories
                        .Where(c => c.ParentCategoryId == null)
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.Name)
                        .ToList();

                    // Recursive function to build tree
                    List<CategoryDto> BuildTree(IEnumerable<Domain.Entities.Category> categories)
                    {
                        var result = new List<CategoryDto>();
                        
                        foreach (var category in categories)
                        {
                            var dto = _mapper.Map<CategoryDto>(category);
                            dto.ProductCount = category.Products.Count;
                            
                            // Get subcategories
                            var subCategories = allCategories
                                .Where(c => c.ParentCategoryId == category.Id)
                                .OrderBy(c => c.DisplayOrder)
                                .ThenBy(c => c.Name)
                                .ToList();
                            
                            dto.SubCategoryCount = subCategories.Count;
                            
                            // Recursively build subcategory tree
                            if (subCategories.Any())
                            {
                                dto.SubCategories = BuildTree(subCategories);
                            }
                            
                            result.Add(dto);
                        }
                        
                        return result;
                    }

                    return BuildTree(rootCategories);
                },
                TimeSpan.FromMinutes(30), // Longer cache for tree structure
                cancellationToken);

            _logger.LogInformation("Retrieved category tree with {Count} root categories", categoryTree.Count);

            return Result<List<CategoryDto>>.Success(categoryTree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category tree");
            return Result<List<CategoryDto>>.Failure($"Failed to retrieve category tree: {ex.Message}");
        }
    }
}
