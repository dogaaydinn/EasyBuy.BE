using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Basket : BaseEntity<int>
{
    public int AppUserId { get; set; } // Foreign Key
    public AppUser AppUser { get; set; } // Navigation Property
    public List<BasketItem> Items { get; set; } = new List<BasketItem>();
}
