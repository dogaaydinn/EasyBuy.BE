using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities.Identity;

public class AppUser : BaseEntity<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Email Email { get; set; } // Value Object
    public PhoneNumber PhoneNumber { get; set; } // Value Object
    public Address Address { get; set; } // Value Object
}
