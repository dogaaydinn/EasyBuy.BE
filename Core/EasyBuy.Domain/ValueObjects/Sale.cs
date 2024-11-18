using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Sale : ValueObject
{
    public decimal DiscountPercentage { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DiscountPercentage;
        yield return StartDate;
        yield return EndDate;
    }
}