using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Review : BaseEntity
{
    public required Guid ProductId { get; set; }
    public required Product Product { get; set; }
    public required Guid UserId { get; set; }
    public required AppUser User { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    public int HelpfulCount { get; set; }
    public bool IsApproved { get; set; }
}
