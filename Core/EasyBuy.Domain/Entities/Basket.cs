using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Basket : BaseEntity
{
    public Guid AppUserId { get; }
    public AppUser AppUser { get; private set; }
    public List<BasketItem> Items { get; }
    public IReadOnlyCollection<BasketItem> ItemsList => Items.AsReadOnly();

    public void AddItem(BasketItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Item cannot be null.");

        if (Items.Contains(item))
            throw new InvalidOperationException("Item is already in the basket.");

        Items.Add(item);
    }

    public void RemoveItem(BasketItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Item cannot be null.");

        if (!Items.Contains(item))
            throw new InvalidOperationException("Item is not in the basket.");

        Items.Remove(item);
    }

    private decimal GetTotalPrice()
    {
        return Items.Sum(item => item.Price * item.Quantity);
    }

    public override string ToString()
    {
        return $"Basket for User {AppUserId} with {Items.Count} items, Total Price: {GetTotalPrice():C}.";
    }
}