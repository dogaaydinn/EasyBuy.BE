using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class ProductDescription : ValueObject
{
    public ProductDescription(string description)
    {
        Guard.AgainstNullOrWhiteSpace(description, nameof(description));
        Description = description;
    }

    public string Description { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Description;
    }
}