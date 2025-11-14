namespace EasyBuy.Application.Repositories.Category;

/// <summary>
/// Read repository for Category entity.
/// Supports hierarchical queries for parent/child relationships.
/// </summary>
public interface ICategoryReadRepository : IReadRepository<Domain.Entities.Category>
{
    /// <summary>
    /// Get all root categories (categories without parent).
    /// </summary>
    Task<IEnumerable<Domain.Entities.Category>> GetRootCategoriesAsync();
    
    /// <summary>
    /// Get all subcategories of a parent category.
    /// </summary>
    Task<IEnumerable<Domain.Entities.Category>> GetSubCategoriesAsync(Guid parentCategoryId);
    
    /// <summary>
    /// Get category tree starting from root categories.
    /// </summary>
    Task<IEnumerable<Domain.Entities.Category>> GetCategoryTreeAsync();
}
