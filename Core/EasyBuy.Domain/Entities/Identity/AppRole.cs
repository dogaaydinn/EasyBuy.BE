using Microsoft.AspNetCore.Identity;

namespace EasyBuy.Domain.Entities.Identity;

/// <summary>
/// Application role entity for role-based authorization
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    public AppRole() : base()
    {
    }

    public AppRole(string roleName) : base(roleName)
    {
    }

    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
