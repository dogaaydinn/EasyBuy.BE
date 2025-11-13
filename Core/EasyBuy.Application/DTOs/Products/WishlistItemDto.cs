namespace EasyBuy.Application.DTOs.Products;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedDate { get; set; }
}
