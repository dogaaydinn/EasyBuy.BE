using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Price : ValueObject
{
    public Price(decimal amount, string currency)
    {
        Guard.AgainstNullOrEmpty(currency, nameof(currency));
        Guard.AgainstOutOfRange(amount, 0, decimal.MaxValue, nameof(amount)); 

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}