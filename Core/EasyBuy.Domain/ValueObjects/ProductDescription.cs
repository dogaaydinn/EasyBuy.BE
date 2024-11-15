namespace EasyBuy.Domain.ValueObjects;

public class ProductDescription : IEquatable<ProductDescription>
{
    public ProductDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product description cannot be empty.", nameof(value));
        if (value.Length > 500)
            throw new ArgumentException("Product description cannot exceed 500 characters.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(ProductDescription other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ProductDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(ProductDescription left, ProductDescription right)
    {
        return left.Equals(right);
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