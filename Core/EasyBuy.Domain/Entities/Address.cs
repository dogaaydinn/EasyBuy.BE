using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Address : BaseEntity
{
    public required Guid UserId { get; set; }
    public required AppUser User { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
    public bool IsDefault { get; set; }
    public AddressType Type { get; set; }
}

public enum AddressType
{
    Shipping,
    Billing,
    Both
}
