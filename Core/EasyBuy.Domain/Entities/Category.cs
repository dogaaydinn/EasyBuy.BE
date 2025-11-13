using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
