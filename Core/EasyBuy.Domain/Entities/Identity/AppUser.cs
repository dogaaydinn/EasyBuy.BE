using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities.Identity;

public class AppUser : BaseEntity
{
    [StringLength(127)]
    public required string FirstName { get; set; } 
    [StringLength(127)]
    public required string LastName { get; set; } 
    public required string Email { get; set; } // Value Object
    public required string PhoneNumber { get; set; } // Value Object
    public required Address Address { get; set; } // Value Object
}