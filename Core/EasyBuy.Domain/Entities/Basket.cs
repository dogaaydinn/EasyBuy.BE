using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBuy.Domain.Entities
{
    public class Basket : BaseEntity<int>
    {
        public Basket(int appUserId, AppUser appUser)
        {
            if (appUserId <= 0) throw new ArgumentException("AppUserId must be greater than zero.", nameof(appUserId));
            AppUserId = appUserId;
            AppUser = appUser ?? throw new ArgumentNullException(nameof(appUser), "AppUser cannot be null.");
            Items = new List<BasketItem>();
        }

        public int AppUserId { get; }
        public AppUser AppUser { get; private set; }
        private List<BasketItem> Items { get; }

        // Exposing Items as IReadOnlyCollection to ensure it can't be modified directly
        public IReadOnlyCollection<BasketItem> ItemsList => Items.AsReadOnly();

        public void AddItem(BasketItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            Items.Add(item);
        }

        public void RemoveItem(BasketItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            Items.Remove(item);
        }

        // Public method to get the total price
        public decimal GetTotalPrice()
        {
            return Items.Sum(item => item.Price * item.Quantity);
        }

        public override string ToString()
        {
            return $"Basket for User {AppUserId} with {Items.Count} items, Total Price: {GetTotalPrice():C}.";
        }
    }
}