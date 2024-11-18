using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class DeliveryMethod : BaseEntity
{
   
   [StringLength(127)]
    public string ShortName { get; }
    [StringLength(255)]
    public string Description { get; }
    
    [DataType(DataType.Currency)]
    public decimal Price { get; }
    [DataType(DataType.Duration)]
    public TimeSpan DeliveryTime { get; }
}