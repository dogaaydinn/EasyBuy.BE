using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public static Quantity Create(int value)
    {
        // İş kuralı: Üst limit kontrolü
        if (value > 10000)
            throw new ArgumentException("Quantity cannot exceed 10,000.", nameof(value));

        return new Quantity(value);
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}