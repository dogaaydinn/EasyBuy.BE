namespace EasyBuy.Domain.ValueObjects;

public class ProductName : IEquatable<ProductName>
{
    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name cannot be empty.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Product name cannot exceed 100 characters.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(ProductName other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ProductName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(ProductName left, ProductName right)
    {
        return left.Equals(right);
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