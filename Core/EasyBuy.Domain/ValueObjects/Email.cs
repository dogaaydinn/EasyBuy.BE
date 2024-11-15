using EasyBuy.Domain.Primitives;

public class Email : ValueObject
{
    public string Address { get; }

    public Email(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || !address.Contains("@"))
        {
            throw new ArgumentException("Invalid email address", nameof(address));
        }

        Address = address;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address.ToLower(); // Case-insensitive comparison
    }

    public override string ToString()
    {
        return Address;
    }
}
