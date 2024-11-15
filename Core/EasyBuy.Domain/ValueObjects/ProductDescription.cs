using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ProductDescription : ValueObject
{
    public string Value { get; }

    private ProductDescription(string value)
    {
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(value));
    }

    public static ProductDescription Create(string value)
    {
        // İş kuralı: Maksimum uzunluk
        if (value.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(value));

        return new ProductDescription(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}