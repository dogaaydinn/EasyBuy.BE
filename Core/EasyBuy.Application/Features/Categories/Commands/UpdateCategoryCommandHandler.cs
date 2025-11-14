using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Repositories.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Handler for UpdateCategoryCommand.
/// Updates category and invalidates cache.
/// </summary>
public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<bool>>
{
    private readonly ICategoryWriteRepository _writeRepository;
    private readonly ICategoryReadRepository _readRepository;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ICategoryWriteRepository writeRepository,
        ICategoryReadRepository readRepository,
        ILayeredCacheService cache,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category: {CategoryId}", request.CategoryId);

        try
        {
            // Get existing category
            var category = await _readRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return Result<bool>.Failure($"Category not found: {request.CategoryId}");
            }

            // Validate parent category if specified
            if (request.ParentCategoryId.HasValue)
            {
                // Prevent circular references
                if (request.ParentCategoryId.Value == request.CategoryId)
                {
                    return Result<bool>.Failure("Category cannot be its own parent");
                }

                // Check if new parent exists
                var parentCategory = await _readRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    return Result<bool>.Failure($"Parent category not found: {request.ParentCategoryId}");
                }

                // Prevent making a category its own descendant
                if (await IsDescendantOf(request.CategoryId, request.ParentCategoryId.Value))
                {
                    return Result<bool>.Failure("Cannot move category under its own descendant");
                }
            }

            // Check for duplicate name at the same level (excluding current category)
            var existingCategories = await _readRepository.GetAllAsync();
            var duplicate = existingCategories.FirstOrDefault(c => 
                c.Id != request.CategoryId &&
                c.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && 
                c.ParentCategoryId == request.ParentCategoryId);

            if (duplicate != null)
            {
                return Result<bool>.Failure($"Category with name '{request.Name}' already exists at this level");
            }

            // Update category
            category.Name = request.Name;
            category.Description = request.Description;
            category.ImageUrl = request.ImageUrl;
            category.ParentCategoryId = request.ParentCategoryId;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;

            await _writeRepository.UpdateAsync(category);

            // Invalidate cache
            await _cache.RemoveAsync($"category:{request.CategoryId}", cancellationToken);
            await _cache.RemoveAsync("categories:all", cancellationToken);
            await _cache.RemoveAsync("categories:tree", cancellationToken);

            _logger.LogInformation("Category updated successfully: {CategoryId}", request.CategoryId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {CategoryId}", request.CategoryId);
            return Result<bool>.Failure($"Failed to update category: {ex.Message}");
        }
    }

    private async Task<bool> IsDescendantOf(Guid categoryId, Guid potentialAncestorId)
    {
        var category = await _readRepository.GetByIdAsync(potentialAncestorId);
        
        while (category != null && category.ParentCategoryId.HasValue)
        {
            if (category.ParentCategoryId.Value == categoryId)
            {
                return true;
            }
            category = await _readRepository.GetByIdAsync(category.ParentCategoryId.Value);
        }
        
        return false;
    }
}
