using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public PostalCode PostalCode { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}