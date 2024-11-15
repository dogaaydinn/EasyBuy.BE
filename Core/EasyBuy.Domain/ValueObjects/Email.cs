using System.Net.Mail;

namespace EasyBuy.Domain.ValueObjects;

public readonly struct Email : IEquatable<Email>
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Email cannot be null or empty.", nameof(value));
        if (!IsValidEmail(value)) throw new ArgumentException("Invalid email format.", nameof(value));

        Value = value;
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var mailAddress = new MailAddress(value);
            return mailAddress.Address == value;
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is Email other && Equals(other);
    }

    public bool Equals(Email other)
    {
        return Value == other.Value;
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
}