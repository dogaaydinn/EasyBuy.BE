using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EasyBuy.Domain.Entities.Identity;

/// <summary>
/// Application user entity extending IdentityUser for authentication
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    [StringLength(127)]
    public string? FirstName { get; set; }

    [StringLength(127)]
    public string? LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}