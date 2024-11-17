using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Quantity(int value) : RangedValueObject<int>(value, 1, int.MaxValue)
{
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}