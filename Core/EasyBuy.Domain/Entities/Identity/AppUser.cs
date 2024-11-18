using System.ComponentModel.DataAnnotations;
using EasyBuy.Domain.Primitives;
using EasyBuy.Domain.ValueObjects;

namespace EasyBuy.Domain.Entities.Identity;

public class AppUser : BaseEntity
{
    [StringLength(127)]
    public string FirstName { get; set; }
    [StringLength(127)]
    public string LastName { get; set; }
    public Email Email { get; set; }
    public PhoneNumber PhoneNumber { get; set; }
    public Address Address { get; set; }
}