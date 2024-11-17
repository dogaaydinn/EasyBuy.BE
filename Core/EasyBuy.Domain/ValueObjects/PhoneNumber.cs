using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public PhoneNumber(string countryCode, string number)
    {
        Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode)); 
        Guard.AgainstNullOrWhiteSpace(number, nameof(number)); 

        CountryCode = countryCode;
        Number = number;
    }

    public string CountryCode { get; }
    public string Number { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
}