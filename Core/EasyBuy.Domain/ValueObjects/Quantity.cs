using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Quantity : ValueObject
{
    private Quantity(int value)
    {
        Value = Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public int Value { get; }

    public static Quantity Create(int value)
    {
        if (value > 10000)
            throw new ArgumentException("Quantity cannot exceed 10,000.", nameof(value));

        return new Quantity(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}