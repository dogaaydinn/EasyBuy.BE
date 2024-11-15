using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities.Identity;

public class AppUser : BaseEntity<int>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public Email? Email { get; init; } 
    public PhoneNumber? PhoneNumber { get; init; } 
    public Address? Address { get; init; } 
}