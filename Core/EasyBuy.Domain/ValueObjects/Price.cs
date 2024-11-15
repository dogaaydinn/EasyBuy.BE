namespace EasyBuy.Domain.ValueObjects;

public readonly struct Price : IEquatable<Price>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Price(decimal amount, string currency)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public override bool Equals(object obj)
    {
        return obj is Price other && Equals(other);
    }

    public bool Equals(Price other)
    {
        return Amount == other.Amount && Currency == other.Currency;
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
}