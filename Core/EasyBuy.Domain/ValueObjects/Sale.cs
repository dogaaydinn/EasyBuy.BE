using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.ValueObjects;

public class Sale : ValueObject
{
    private Sale(decimal discountPercentage, DateTime startDate, DateTime endDate)
    {
        if (discountPercentage is < 0 or > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.");

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date.");

        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
    }

    public decimal DiscountPercentage { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public static Sale Create(decimal discountPercentage, DateTime startDate, DateTime endDate)
    {
        return new Sale(discountPercentage, startDate, endDate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DiscountPercentage;
        yield return StartDate;
        yield return EndDate;
    }
}