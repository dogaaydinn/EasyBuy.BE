using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Address : ValueObject
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}