using EasyBuy.Application.Repositories.Category;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Persistence.Repositories.Categories;

/// <summary>
/// Entity Framework implementation of ICategoryReadRepository.
/// Provides hierarchical category queries with eager loading.
/// </summary>
public class EfCategoryReadRepository(EasyBuyDbContext dbContext)
    : EfReadRepository<Category>(dbContext), ICategoryReadRepository
{
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await dbContext.Set<Category>()
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId)
    {
        return await dbContext.Set<Category>()
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
    {
        // Get all categories
        var allCategories = await dbContext.Set<Category>()
            .Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // Return only root categories (EF Core will include subcategories via navigation)
        return allCategories.Where(c => c.ParentCategoryId == null);
    }
}
