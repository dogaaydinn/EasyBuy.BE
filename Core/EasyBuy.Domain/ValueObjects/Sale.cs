using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Sale : ValueObject
{
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DiscountPercentage;
        yield return StartDate;
        yield return EndDate;
    }
}