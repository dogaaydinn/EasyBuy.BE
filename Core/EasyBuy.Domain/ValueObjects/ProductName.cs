using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ProductName(string value) : LengthRestrictedValueObject(value, MaxLength), IEquatable<ProductName>
{
    private const int MaxLength = 100;

    public bool Equals(ProductName? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ProductName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public static bool operator ==(ProductName left, ProductName right)
    {
        return left?.Equals(right) ?? ReferenceEquals(right, null);
    }

    public static bool operator !=(ProductName left, ProductName right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}