using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ProductDescription(string value)
    : LengthRestrictedValueObject(value, MaxLength), IEquatable<ProductDescription>
{
    private const int MaxLength = 500; 
    
    public bool Equals(ProductDescription? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ProductDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public static bool operator ==(ProductDescription left, ProductDescription right)
    {
        return left?.Equals(right) ?? ReferenceEquals(right, null);
    }

    public static bool operator !=(ProductDescription left, ProductDescription right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}