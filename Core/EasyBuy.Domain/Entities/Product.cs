using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class Product(
    ProductName name,
    Guid productTypeId,
    Guid productBrandId,
    decimal price,
    Guid id,
    ProductDescription description,
    Quantity quantity,
    Sale sale,
    ProductType productType,
    ProductBrand productBrand)
    : Entity<Guid>(id)
{
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ProductType ProductType { get; set; } = productType;
    public ProductBrand ProductBrand { get; set; } = productBrand;
    public ProductName Name { get; } = name ?? throw new ArgumentNullException(nameof(name), "Product name cannot be null.");
    public Guid ProductTypeId { get; } = productTypeId != Guid.Empty
        ? productTypeId
        : throw new ArgumentException("Invalid ProductTypeId", nameof(productTypeId));

    public Guid ProductBrandId { get; } = productBrandId != Guid.Empty
        ? productBrandId
        : throw new ArgumentException("Invalid ProductBrandId", nameof(productBrandId));

    public decimal Price { get; } = price >= 0 ? price : throw new ArgumentException("Price must be non-negative", nameof(price));
    public string? PictureUrl { get; set; }
    public Sale Sale { get; private set; } = sale ?? throw new ArgumentNullException(nameof(sale));
    public ICollection<ProductOrder> ProductOrders { get; set; } = new List<ProductOrder>();

    public ProductDescription Description { get; } = description ?? throw new ArgumentNullException(nameof(description));
    public Quantity Quantity { get; } = quantity ?? throw new ArgumentNullException(nameof(quantity));

    public override string ToString()
    {
        return $"{Name} - {Price:C}";
    }
}