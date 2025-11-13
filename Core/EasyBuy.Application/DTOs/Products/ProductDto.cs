using EasyBuy.Domain.Enums;

namespace EasyBuy.Application.DTOs.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ProductType ProductType { get; set; }
    public string? Brand { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsInStock => Quantity > 0;
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsInStock { get; set; }
}

public class ProductDetailDto : ProductDto
{
    public string? CategoryName { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<ProductDto> RelatedProducts { get; set; } = new();
}

public class CreateProductDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ProductType ProductType { get; set; }
    public string? Brand { get; set; }
    public Guid? CategoryId { get; set; }
}

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public ProductType? ProductType { get; set; }
    public string? Brand { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public DateTime ReviewDate { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
}
