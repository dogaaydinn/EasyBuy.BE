namespace EasyBuy.Domain.Primitives;

public abstract class RangedValueObject<T> : ValueObject where T : IComparable<T>
{
    public T Value { get; }
    public T Min { get; }
    public T Max { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Min;
        yield return Max;
    }
}