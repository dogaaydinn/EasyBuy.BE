namespace EasyBuy.Domain.Primitives;

public abstract class LengthRestrictedValueObject : IEquatable<LengthRestrictedValueObject>
{
    protected LengthRestrictedValueObject(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        if (value.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters. Provided value: '{value}'",
                nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(LengthRestrictedValueObject? other)
    {
        return other != null && GetType() == other.GetType() && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LengthRestrictedValueObject);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override string ToString()
    {
        return $"Value: {Value}";
    }

    public static bool operator ==(LengthRestrictedValueObject? left, LengthRestrictedValueObject right)
    {
        return left?.Equals(right) ?? ReferenceEquals(right, null);
    }

    public static bool operator !=(LengthRestrictedValueObject? left, LengthRestrictedValueObject right)
    {
        return !(left == right);
    }
}