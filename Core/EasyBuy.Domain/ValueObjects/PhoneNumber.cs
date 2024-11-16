namespace EasyBuy.Domain.ValueObjects;

public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException(nameof(number), "Phone number cannot be null or empty.");

        CountryCode = countryCode;
        Number = number;
    }

    public string CountryCode { get; }
    public string Number { get; }

    public bool Equals(PhoneNumber? other)
    {
        return CountryCode == other?.CountryCode && Number == other.Number;
    }

    public override bool Equals(object? obj)
    {
        return obj is PhoneNumber other && Equals(other);
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