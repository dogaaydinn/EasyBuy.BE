using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Quantity(int value) : ValueObject
{
    public int Value { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
