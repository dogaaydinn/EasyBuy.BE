using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Delivery : BaseEntity
{
    [StringLength(127)]
    public required string ShortName { get; set; } // eg. DHL, UPS, FedEx
    
    [StringLength(127)]
    public required string Description { get; set; } 
    
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    
    [DataType(DataType.Duration)]
    public TimeSpan DeliveryTime { get; set; } 
}