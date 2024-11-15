using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 10)
            throw new ArgumentException("Invalid phone number.");

        Value = value;
    }

    public static PhoneNumber Create(string value) => new PhoneNumber(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
