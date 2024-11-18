using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class DeliveryMethod : BaseEntity
{
   
   [StringLength(127)]
    public string ShortName { get; set; }
    [StringLength(255)]
    public string Description { get; set; }
    
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    [DataType(DataType.Duration)]
    public TimeSpan DeliveryTime { get; set; }
}