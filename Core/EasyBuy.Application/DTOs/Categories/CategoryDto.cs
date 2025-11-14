namespace EasyBuy.Application.DTOs.Categories;

/// <summary>
/// Data transfer object for Category entity.
/// Includes hierarchical information and product count.
/// </summary>
public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public int SubCategoryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Subcategories for hierarchical tree queries.
    /// Null if not loaded.
    /// </summary>
    public List<CategoryDto>? SubCategories { get; set; }
}
