using System.Text.RegularExpressions;

namespace EasyBuy.Domain.ValueObjects;

public sealed class PostalCode : IEquatable<PostalCode>
{
    private const int MaxLength = 10;
        
    private static readonly Regex PostalCodeRegex = new Regex(@"^[A-Za-z0-9]{1,10}$", RegexOptions.Compiled);

    public PostalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "Postal code cannot be null or empty.");

        if (value.Length > MaxLength)
            throw new ArgumentException($"Postal code cannot exceed {MaxLength} characters.", nameof(value));
            
        if (!PostalCodeRegex.IsMatch(value))
            throw new ArgumentException("Postal code format is invalid.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(PostalCode? other)
    {
        return Value == other?.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is PostalCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(PostalCode left, PostalCode right)
    {
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(PostalCode left, PostalCode right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}