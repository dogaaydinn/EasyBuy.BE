namespace EasyBuy.Application.DTOs.Baskets;

public class BasketDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(x => x.Total);
    public decimal Tax => SubTotal * 0.1m; // 10% tax
    public decimal Total => SubTotal + Tax;
    public int ItemCount => Items.Sum(x => x.Quantity);
}

public class BasketItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
    public string? ImageUrl { get; set; }
    public bool IsInStock { get; set; }
}

public class AddToBasketDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBasketItemDto
{
    public Guid BasketItemId { get; set; }
    public int Quantity { get; set; }
}
