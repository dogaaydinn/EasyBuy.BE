using System.Text.RegularExpressions;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private bool Equals(Email other)
    {
        return base.Equals(other) && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Email other && Equals(other);
    }

    public Email(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), "Email cannot be null or empty.");
        if (!IsValidEmail(value)) 
            throw new FormatException("Invalid email format.");

        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValidEmail(string value)
    {
        const string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return MyRegex().IsMatch(value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(Email left, Email right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Email left, Email right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}