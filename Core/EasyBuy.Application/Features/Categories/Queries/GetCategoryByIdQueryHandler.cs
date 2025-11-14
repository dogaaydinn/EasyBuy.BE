using AutoMapper;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.DTOs.Categories;
using EasyBuy.Application.Repositories.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Queries;

/// <summary>
/// Handler for GetCategoryByIdQuery.
/// Retrieves single category with caching.
/// </summary>
public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryReadRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        ICategoryReadRepository repository,
        IMapper mapper,
        ILayeredCacheService cache,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category: {CategoryId}", request.CategoryId);

        try
        {
            var categoryDto = await _cache.GetOrSetAsync(
                $"category:{request.CategoryId}",
                async () =>
                {
                    var category = await _repository.GetByIdAsync(request.CategoryId);
                    if (category == null)
                    {
                        return null;
                    }

                    var dto = _mapper.Map<CategoryDto>(category);
                    
                    // Enrich with counts and parent name
                    dto.ProductCount = category.Products.Count;
                    dto.SubCategoryCount = category.SubCategories.Count;
                    
                    if (category.ParentCategoryId.HasValue)
                    {
                        var parentCategory = await _repository.GetByIdAsync(category.ParentCategoryId.Value);
                        dto.ParentCategoryName = parentCategory?.Name;
                    }

                    return dto;
                },
                TimeSpan.FromMinutes(10),
                cancellationToken);

            if (categoryDto == null)
            {
                return Result<CategoryDto>.Failure($"Category not found: {request.CategoryId}");
            }

            _logger.LogInformation("Retrieved category: {CategoryId}, Name: {CategoryName}", 
                categoryDto.Id, categoryDto.Name);

            return Result<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category: {CategoryId}", request.CategoryId);
            return Result<CategoryDto>.Failure($"Failed to retrieve category: {ex.Message}");
        }
    }
}
