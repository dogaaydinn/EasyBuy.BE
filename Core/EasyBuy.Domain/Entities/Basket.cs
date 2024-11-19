using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Basket : BaseEntity
{
    public required AppUser AppUser { get; set; } // Value Object
    public ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();
}