using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Address { get; set; }
        
    public string Value => Address; 

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
    }
}