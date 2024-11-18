using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public Email Email { get; }
    public PhoneNumber? PhoneNumber { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber != null ? PhoneNumber : string.Empty;
    }
}