using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ProductDescription : ValueObject
{
    public string Description { get; set;}

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Description;
    }
}