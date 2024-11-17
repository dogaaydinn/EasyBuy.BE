using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Email : ValueObject
{
    public Email(string address)
    {
        Guard.AgainstNullOrWhiteSpace(address, nameof(address));
        Address = address;
    }

    public string Address { get; }
        
    public string Value => Address; 

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
    }
}