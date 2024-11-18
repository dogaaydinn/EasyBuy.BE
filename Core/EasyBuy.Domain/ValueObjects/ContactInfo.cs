using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public Email Email { get; set; }
    public PhoneNumber? PhoneNumber { get; set;}

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber != null ? PhoneNumber : string.Empty;
    }
}