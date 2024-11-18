using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string CountryCode { get; set; }
    public string Number { get; set; }


    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
}