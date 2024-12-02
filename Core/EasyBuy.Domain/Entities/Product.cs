using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Enums;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Product : BaseEntity
{
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }  
    public string? PictureUrl { get; set; } 
    public required ProductType ProductType { get; set; }  
    public string ProductBrand { get; set; } 
    public required string Name { get; set; } 
    public bool OnSale { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>(); 
}