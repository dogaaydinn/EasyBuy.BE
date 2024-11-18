using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class PostalCode : ValueObject
{
    public string Code { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}