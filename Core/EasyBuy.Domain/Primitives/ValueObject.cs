namespace EasyBuy.Domain.Primitives;

public abstract class ValueObject
{
    //ValueObject, domain içinde bir değer türünü temsil eden, değiştirilemez (immutable) ve bir kimlik (Id) yerine değerleriyle eşitliği tanımlanan bir yapıdır.
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(valueObject.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                return HashCode.Combine(current, obj);
            });
    }
}