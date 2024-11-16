using System.Text.RegularExpressions;
using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public sealed partial class Price : ValueObject
{
    private bool Equals(Price other)
    {
        return base.Equals(other) && Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Price other && Equals(other);
    }

    public Price(decimal amount, string currency)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency), "Currency cannot be null or empty.");

        // ISO 4217 currency code validation
        const string currencyRegex = @"^[A-Z]{3}$";
        if (!MyRegex().IsMatch(currency))
            throw new ArgumentException("Invalid currency code. Use ISO 4217 format.", nameof(currency));

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

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public static bool operator ==(Price left, Price right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Price left, Price right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex MyRegex();
}