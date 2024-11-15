using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Price : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Price(decimal amount, string currency)
    {
        Amount = Guard.Against.NegativeOrZero(amount, nameof(amount));
        Currency = Guard.Against.NullOrEmpty(currency, nameof(currency));
    }

    public static Price Create(decimal amount, string currency)
    {
        // İş kuralı: Belirli bir para birimi formatı kontrolü
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a valid ISO 4217 code.", nameof(currency));

        return new Price(amount, currency);
    }

    public override string ToString() => $"{Amount} {Currency}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}