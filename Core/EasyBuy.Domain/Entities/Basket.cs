using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Basket : BaseEntity<int>
{
    public Basket(int appUserId)
    {
        if (appUserId <= 0) throw new ArgumentException("AppUserId must be greater than zero.", nameof(appUserId));

        AppUserId = appUserId;
        Items = new List<BasketItem>();
    }

    public int AppUserId { get; } 
    public AppUser AppUser { get; private set; }
    public IReadOnlyCollection<BasketItem> Items { get; }

    // Business logic method: Add item to basket
    // Business logic method: Remove item from basket
    
    public override string ToString()
    {
        return $"Basket for User {AppUserId} with {Items.Count} items.";
    }
}