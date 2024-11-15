using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class DeliveryMethod : BaseEntity<int>
{
    public string ShortName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public TimeSpan DeliveryTime { get; set; }
}