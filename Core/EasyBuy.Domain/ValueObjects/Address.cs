using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Address : ValueObject
{
    public Address(string street, string city, string state, PostalCode postalCode)
    {
        Guard.AgainstNullOrWhiteSpace(street, nameof(street));
        Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        Guard.AgainstNullOrWhiteSpace(state, nameof(state));
        Guard.AgainstNullOrWhiteSpace(postalCode?.Code ?? throw new ArgumentNullException(nameof(postalCode)), nameof(postalCode.Code));

        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public PostalCode PostalCode { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}