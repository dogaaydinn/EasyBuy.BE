namespace EasyBuy.Domain.ValueObjects;

public readonly struct Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }

    public Address(string street, string city, string state, string postalCode)
    {
        if (string.IsNullOrEmpty(street))
            throw new ArgumentException("Street cannot be null or empty.", nameof(street));
        if (string.IsNullOrEmpty(city)) throw new ArgumentException("City cannot be null or empty.", nameof(city));
        if (string.IsNullOrEmpty(state)) throw new ArgumentException("State cannot be null or empty.", nameof(state));
        if (string.IsNullOrEmpty(postalCode))
            throw new ArgumentException("PostalCode cannot be null or empty.", nameof(postalCode));

        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public override bool Equals(object obj)
    {
        return obj is Address other && Equals(other);
    }

    public bool Equals(Address other)
    {
        return Street == other.Street && City == other.City && State == other.State && PostalCode == other.PostalCode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, PostalCode);
    }

    public static bool operator ==(Address left, Address right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Address left, Address right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State}, {PostalCode}";
    }
}