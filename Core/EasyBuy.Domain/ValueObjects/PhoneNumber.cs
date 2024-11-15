namespace EasyBuy.Domain.ValueObjects;

public readonly struct PhoneNumber : IEquatable<PhoneNumber>
{
    public string CountryCode { get; }
    public string Number { get; }

    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrEmpty(countryCode))
            throw new ArgumentException("Country code cannot be null or empty.", nameof(countryCode));
        if (string.IsNullOrEmpty(number))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(number));

        CountryCode = countryCode;
        Number = number;
    }

    public override bool Equals(object obj)
    {
        return obj is PhoneNumber other && Equals(other);
    }

    public bool Equals(PhoneNumber other)
    {
        return CountryCode == other.CountryCode && Number == other.Number;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CountryCode, Number);
    }

    public static bool operator ==(PhoneNumber left, PhoneNumber right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber left, PhoneNumber right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{CountryCode} {Number}";
    }
}