using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Wishlist : BaseEntity
{
    public required Guid UserId { get; set; }
    public required AppUser User { get; set; }
    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

public class WishlistItem : BaseEntity
{
    public required Guid WishlistId { get; set; }
    public required Wishlist Wishlist { get; set; }
    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
}
