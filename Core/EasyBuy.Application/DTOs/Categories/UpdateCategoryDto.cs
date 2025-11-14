namespace EasyBuy.Application.DTOs.Categories;

/// <summary>
/// Data transfer object for updating an existing category.
/// </summary>
public sealed class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
