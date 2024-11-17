using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public ContactInfo(Email email, PhoneNumber? phoneNumber)
    {
        Guard.AgainstNull(email, nameof(email));
        if (phoneNumber != null) Guard.AgainstNull(phoneNumber, nameof(phoneNumber));

        Email = email;
        PhoneNumber = phoneNumber;
    }

    public Email Email { get; }
    public PhoneNumber? PhoneNumber { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber != null ? PhoneNumber : string.Empty;
    }
}