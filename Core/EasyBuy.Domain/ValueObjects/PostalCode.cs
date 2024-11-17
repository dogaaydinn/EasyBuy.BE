using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class PostalCode : ValueObject
{
    public PostalCode(string code)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Code = code;
    }

    public string Code { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}