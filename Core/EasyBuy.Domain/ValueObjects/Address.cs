using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }

    private Address(string street, string city, string state, string postalCode)
    {
        Street = Guard.Against.NullOrEmpty(street, nameof(street));
        City = Guard.Against.NullOrEmpty(city, nameof(city));
        State = Guard.Against.NullOrEmpty(state, nameof(state));
        PostalCode = Guard.Against.NullOrEmpty(postalCode, nameof(postalCode));
    }

    public static Address Create(string street, string city, string state, string postalCode)
    {
        // İş kuralı: Posta kodu uzunluğu kontrolü
        if (postalCode.Length != 5)
            throw new ArgumentException("PostalCode must be 5 characters long.", nameof(postalCode));

        return new Address(street, city, state, postalCode);
    }

    public override string ToString() => $"{Street}, {City}, {State}, {PostalCode}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}