using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public sealed partial class Address : ValueObject, IEquatable<Address>
{
    public Address(string street, string city, string state, PostalCode postalCode)
    {
        Guard.AgainstNullOrWhiteSpace(street, nameof(street));
        Guard.AgainstNullOrWhiteSpace(city, nameof(city));
        Guard.AgainstNullOrWhiteSpace(state, nameof(state));

        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode), "PostalCode cannot be null.");
    }

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public PostalCode PostalCode { get; }

    public bool Equals(Address other)
    {
        return Street == other.Street && City == other.City && State == other.State && PostalCode.Equals(other.PostalCode);
    }

    public override bool Equals(object obj)
    {
        return obj is Address other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, PostalCode);
    }

    public static bool operator ==(Address left, Address right)
    {
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(Address left, Address right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State}, {PostalCode}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}