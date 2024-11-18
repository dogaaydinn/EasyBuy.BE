using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Price : ValueObject
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}