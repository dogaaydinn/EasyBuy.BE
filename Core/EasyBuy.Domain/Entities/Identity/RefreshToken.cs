using Microsoft.AspNetCore.Identity;

namespace EasyBuy.Domain.Entities.Identity;

/// <summary>
/// Refresh token for JWT authentication
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required AppUser User { get; set; }
    public required string Token { get; set; }
    public required string JwtId { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
}
