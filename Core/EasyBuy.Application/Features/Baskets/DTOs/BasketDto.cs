namespace EasyBuy.Application.Features.Baskets.DTOs;

public class BasketDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(item => item.Subtotal);
    public int ItemCount => Items.Sum(item => item.Quantity);
}

public class BasketItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
    public int AvailableStock { get; set; }
}

public class AddToBasketDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBasketItemDto
{
    public int Quantity { get; set; }
}
