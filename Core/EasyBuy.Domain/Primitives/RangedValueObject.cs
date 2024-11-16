
namespace EasyBuy.Domain.Primitives
{
    public abstract class RangedValueObject<T> : ValueObject where T : IComparable<T>
    {
        public readonly T Value;

        protected RangedValueObject(T value, T min, T max)
        {
            Guard.AgainstOutOfRange(value, min, max, nameof(value));
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}