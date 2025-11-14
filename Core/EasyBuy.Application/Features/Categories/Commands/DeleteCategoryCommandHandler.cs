using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Application.Repositories.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EasyBuy.Application.Features.Categories.Commands;

/// <summary>
/// Handler for DeleteCategoryCommand.
/// Performs soft delete (IsActive = false) if category has products/subcategories.
/// Performs hard delete if category is empty and ForceDelete = true.
/// </summary>
public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly ICategoryWriteRepository _writeRepository;
    private readonly ICategoryReadRepository _readRepository;
    private readonly ILayeredCacheService _cache;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        ICategoryWriteRepository writeRepository,
        ICategoryReadRepository readRepository,
        ILayeredCacheService cache,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category: {CategoryId}, ForceDelete: {ForceDelete}", 
            request.CategoryId, request.ForceDelete);

        try
        {
            // Get category
            var category = await _readRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return Result<bool>.Failure($"Category not found: {request.CategoryId}");
            }

            // Check if category has subcategories
            var subCategories = await _readRepository.GetSubCategoriesAsync(request.CategoryId);
            var hasSubCategories = subCategories.Any();

            // Check if category has products
            var hasProducts = category.Products.Any();

            // Determine delete strategy
            if (hasSubCategories || hasProducts)
            {
                if (request.ForceDelete)
                {
                    return Result<bool>.Failure(
                        $"Cannot force delete category with {(hasSubCategories ? "subcategories" : "products")}. " +
                        "Please reassign or delete them first.");
                }

                // Soft delete (deactivate)
                category.IsActive = false;
                await _writeRepository.UpdateAsync(category);

                _logger.LogInformation("Category soft deleted (deactivated): {CategoryId}", request.CategoryId);
            }
            else
            {
                // Hard delete (remove from database)
                await _writeRepository.DeleteAsync(category);

                _logger.LogInformation("Category hard deleted: {CategoryId}", request.CategoryId);
            }

            // Invalidate cache
            await _cache.RemoveAsync($"category:{request.CategoryId}", cancellationToken);
            await _cache.RemoveAsync("categories:all", cancellationToken);
            await _cache.RemoveAsync("categories:tree", cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", request.CategoryId);
            return Result<bool>.Failure($"Failed to delete category: {ex.Message}");
        }
    }
}
