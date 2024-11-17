namespace EasyBuy.Domain.Primitives;

public abstract class RangedValueObject<T> : ValueObject where T : IComparable<T>
{
    protected RangedValueObject(T value, T min, T max)
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Min value cannot be greater than max value.");

        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between {min} and {max}.");

        Value = value;
        Min = min;
        Max = max;
    }

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