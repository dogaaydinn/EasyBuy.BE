using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class DeliveryMethod : BaseEntity<int>
{
    public DeliveryMethod(string shortName, string description, decimal price, TimeSpan deliveryTime)
    {
        if (string.IsNullOrWhiteSpace(shortName))
            throw new ArgumentException("Short name cannot be empty.", nameof(shortName));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (deliveryTime <= TimeSpan.Zero)
            throw new ArgumentException("Delivery time must be a positive value.", nameof(deliveryTime));

        ShortName = shortName;
        Description = description;
        Price = price;
        DeliveryTime = deliveryTime;
    }
        
    public string ShortName { get; }
    public string Description { get; }
    public decimal Price { get; }
    public TimeSpan DeliveryTime { get; }
        
    public override string ToString()
    {
        return $"{ShortName} - {Description}, Price: {Price:C}, Delivery Time: {DeliveryTime}";
    }
}