using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities;

public class Product : BaseEntity
{
    public ICollection<OrderItem> OrderItems { get; set; } 
    public ProductType ProductType { get; set; } 
    public ProductBrand ProductBrand { get; set; } 
    public ProductName Name { get; set; } 
    public Guid ProductTypeId { get; } 

    public Guid ProductBrandId { get; } 

    public decimal Price { get; set; } 
    public string? PictureUrl { get; set; }
    public Sale Sale { get; set; } 
    public ICollection<ProductOrder> ProductOrders { get; set; } 

    public ProductDescription Description { get; set; }
    public Quantity Quantity { get; set; } 

}