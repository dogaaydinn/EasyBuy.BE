using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class Product : BaseEntity<int>
{
    public Product(string name, string description, decimal price, string pictureUrl, int productTypeId,
        int productBrandId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty", nameof(description));
        if (price < 0) throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Price = price;
        PictureUrl = pictureUrl;
        ProductTypeId = productTypeId;
        ProductBrandId = productBrandId;
        Orders = new List<Order>();
    }

    public string Name { get; }
    public string Description { get; }
    public decimal Price { get; }
    public string PictureUrl { get; private set; }
    
    public ProductType ProductType { get; private set; }
    public int ProductTypeId { get; private set; }
    public ProductBrand ProductBrand { get; private set; }
    public int ProductBrandId { get; private set; }
    public IReadOnlyCollection<Order> Orders { get; private set; }

    public decimal GetPriceAfterDiscount(decimal discountPercentage)
    {
        if (discountPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage),
                "Discount percentage must be between 0 and 100.");

        return Price - Price * discountPercentage / 100;
    }

    public override string ToString()
    {
        return $"{Name} - {Description} - {Price:C}";
    }
}